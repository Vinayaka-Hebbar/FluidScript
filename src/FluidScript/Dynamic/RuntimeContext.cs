using FluidScript.Compiler.SyntaxTree;
using System;
using System.Linq;

namespace FluidScript.Dynamic
{
    /// <summary>
    /// Runtime evaluation of Syntax tree 
    /// <list type="bullet">it will be not same as compiled </list>
    /// </summary>
    public sealed class RuntimeContext : Compiler.IExpressionVisitor<object>, Compiler.IStatementVisitor, Reflection.Emit.ITypeProvider
    {
        private readonly LocalScope scope;

        internal bool hasReturn;

        internal readonly object Instance;

        private Func<object> LongJump;

        /// <summary>
        /// New runtime evaluation 
        /// </summary>
        public RuntimeContext(object instance)
        {
            Instance = instance;
            scope = new LocalScope();
        }

        /// <summary>
        /// Gets or Sets value
        /// </summary>
        /// <param name="name">Name to store</param>
        /// <returns>value stored in it</returns>
        public object this[string name]
        {
            get => scope[name];
            set => scope[name] = value;
        }

        /// <summary>
        /// Evaluate the <paramref name="syntaxTree"/>
        /// </summary>
        /// <exception cref="IndexOutOfRangeException"/>
        /// <exception cref="NullReferenceException"/>
        /// <exception cref="MissingMethodException"/>
        public object Invoke(Node syntaxTree)
        {
            switch (syntaxTree)
            {
                case Expression exp:
                    return exp.Accept(this);
                case Statement statement:
                    statement.Accept(this);
                    if (LongJump != null)
                        return LongJump();
                    break;
            }
            return null;
        }

        /// <inheritdoc/>
        public object VisitArrayLiteral(ArrayLiteralExpression node)
        {
            Type type;
            if (node.ArrayType != null)
                type = node.ArrayType.GetType(this);
            else
                type = typeof(IFSObject);
            node.Type = typeof(Collections.List<>).MakeGenericType(type);
            var items = node.Expressions;
            var array = (System.Collections.IList)Activator.CreateInstance(node.Type, new object[] { });
            for (int index = 0; index < items.Length; index++)
            {
                array.Add(items[index].Accept(this));
            }
            return array;
        }

        /// <inheritdoc/>
        public object VisitAssignment(AssignmentExpression node)
        {
            var value = node.Right.Accept(this);
            var left = node.Left;
            string name = null;
            var instance = Instance;
            ExpressionType nodeType = left.NodeType;
            if (nodeType == ExpressionType.Identifier)
            {
                name = left.ToString();
                var variable = scope.Find(name);
                if (variable.Equals(LocalVariable.Empty) == false)
                {
                    scope.Current.Modify(variable, value);
                    return value;
                }
            }
            else if (nodeType == ExpressionType.MemberAccess)
            {
                var exp = (MemberExpression)left;
                instance = exp.Target.Accept(this);
                name = exp.Name;
            }
            else if (nodeType == ExpressionType.Indexer)
            {
                var exp = (IndexExpression)left;
                instance = exp.Target.Accept(this);
                var type = instance.GetType();
                if (typeof(System.Collections.IList).IsAssignableFrom(type))
                {
                    var array = (System.Collections.IList)instance;
                    var args = exp.Arguments.Select(arg => Convert.ToInt32(arg.Accept(this))).ToArray();
                    var first = args.First();
                    array.Insert(first, value);
                }
                return value;
            }
            var memeber = instance.GetType().GetMember(name).FirstOrDefault();
            if (memeber != null)
            {
                if (memeber.MemberType == System.Reflection.MemberTypes.Field)
                {
                    var field = (System.Reflection.FieldInfo)memeber;
                    field.SetValue(instance, value);
                }
                else if (memeber.MemberType == System.Reflection.MemberTypes.Property)
                {
                    var property = (System.Reflection.PropertyInfo)memeber;
                    property.SetValue(instance, value, new object[0]);
                }
            }
            else
            {
                //create new variable
                scope.Create(name, value.GetType(), value);
            }
            return value;
        }

        /// <inheritdoc/>
        public object VisitBinary(BinaryExpression node)
        {
            string opName = null;
            switch (node.NodeType)
            {
                case ExpressionType.Plus:
                    opName = "op_Addition";
                    break;
                case ExpressionType.Minus:
                    opName = "op_Subtraction";
                    break;
                case ExpressionType.Multiply:
                    opName = "op_Multiply";
                    break;
                case ExpressionType.Divide:
                    opName = "op_Division";
                    break;
                case ExpressionType.Percent:
                    opName = "op_Modulus";
                    break;
                case ExpressionType.BangEqual:
                    opName = "op_Inequality";
                    break;
                case ExpressionType.EqualEqual:
                    opName = "op_Equality";
                    break;
                case ExpressionType.Greater:
                    opName = "op_GreaterThan";
                    break;
                case ExpressionType.GreaterEqual:
                    opName = "op_GreaterThanOrEqual";
                    break;
                case ExpressionType.Less:
                    opName = "op_LessThan";
                    break;
                case ExpressionType.LessEqual:
                    opName = "op_LessThanOrEqual";
                    break;
            }
            var left = node.Left.Accept(this);
            var right = node.Right.Accept(this);
            var method = Reflection.TypeUtils.GetOperatorOverload(opName, out Reflection.Emit.Conversion[] conversion, left.GetType(), right.GetType());
            node.Method = method;
            var args = new object[] { left, right };
            for (int i = 0; i < conversion.Length; i++)
            {
                Reflection.Emit.Conversion conv = conversion[i];
                if (conv.HasConversion)
                    args[i] = conv.Method.Invoke(null, new object[] { args[i] });
            }
            return method.Invoke(null, args);
        }

        /// <inheritdoc/>
        public object VisitCall(InvocationExpression node)
        {
            var target = node.Target;
            var instance = Instance;
            object[] args = node.Arguments.Select(arg => arg.Accept(this)).ToArray();
            var types = args.Select(arg => arg.GetType()).ToArray();
            string name = null;
            ExpressionType nodeType = node.Target.NodeType;
            if (nodeType == ExpressionType.Identifier)
            {
                name = target.ToString();
            }
            else if (nodeType == ExpressionType.MemberAccess)
            {
                var exp = (MemberExpression)target;
                instance = exp.Target.Accept(this);
                name = exp.Name;
            }
            Type resultType = instance.GetType();
            bool HasMethod(System.Reflection.MethodInfo m)
            {
                if (m.IsDefined(typeof(Runtime.RegisterAttribute), false))
                {
                    var data = (Attribute)m.GetCustomAttributes(typeof(Runtime.RegisterAttribute), false).FirstOrDefault();
                    if (data != null)
                        return data.Match(name);
                }
                return m.Name == name;
            }
            var methods = resultType
                .GetMethods(Reflection.TypeUtils.Any)
                .Where(HasMethod).ToArray();

            if (methods.Length == 0)
                throw new Exception(string.Concat("method ", name, " not found"));
            var method = Reflection.TypeUtils.BindToMethod(methods, types, out Reflection.Emit.Conversion[] conversion);
            for (int i = 0; i < conversion.Length; i++)
            {
                Reflection.Emit.Conversion conv = conversion[i];
                if (conv.HasConversion)
                    args[i] = conv.Method.Invoke(null, new object[] { args[i] });
            }
            node.Method = method;
            node.Type = method.ReturnType;
            return method.Invoke(instance, args);
        }

        /// <inheritdoc/>
        public object VisitIndex(IndexExpression node)
        {
            var target = node.Target.Accept(this);
            var type = target.GetType();
            object value = null;
            var args = node.Arguments.Select(arg => arg.Accept(this)).ToArray();
            if (typeof(System.Collections.IList).IsAssignableFrom(type))
            {
                var array = (System.Collections.IList)target;
                type = type.GetGenericArguments()[0];
                var first = args.Select(arg => Convert.ToInt32(arg)).FirstOrDefault();
                value = array[first];
            }
            else
            {
                var types = args.Select(arg => arg.GetType()).ToArray();
                var indexer = type.GetProperty("Item", types);
                node.Indexer = indexer ?? throw new Exception("Indexer not found");
                type = indexer.PropertyType;
                value = node.Indexer.GetValue(target, args);
            }
            node.Type = type;
            return value;
        }

        /// <inheritdoc/>
        public object VisitLiteral(LiteralExpression node)
        {
            object value = node.Value;
            switch (value)
            {
                case int _:
                    value = new Integer((int)value);
                    break;
                case double _:
                    value = new Double((double)value);
                    break;
                case string _:
                    value = new String(value.ToString());
                    break;
                case bool _:
                    value = new Boolean((bool)value);
                    break;
                case null:
                    value = null;
                    break;
            }
            node.Type = value.GetType();
            return value;
        }


        bool HasMember(System.Reflection.MemberInfo m, object filter)
        {
            if (m.IsDefined(typeof(Runtime.RegisterAttribute), false))
            {
                var data = (System.Attribute)m.GetCustomAttributes(typeof(Runtime.RegisterAttribute), false).FirstOrDefault();
                if (data != null)
                    return data.Match(filter);
            }
            return filter.Equals(m.Name);
        }

        /// <inheritdoc/>
        public object VisitMember(MemberExpression node)
        {
            var target = node.Target.Accept(this);
            var member = target.GetType().FindMembers(System.Reflection.MemberTypes.Field | System.Reflection.MemberTypes.Property, Reflection.TypeUtils.Any, this.HasMember, node.Name).FirstOrDefault();
            if (member != null)
            {
                if (member.MemberType == System.Reflection.MemberTypes.Field)
                {
                    var field = (System.Reflection.FieldInfo)member;
                    node.Type = field.FieldType;
                    return field.GetValue(target);

                }
                else if (member.MemberType == System.Reflection.MemberTypes.Property)
                {
                    var property = (System.Reflection.PropertyInfo)member;

                    node.Type = property.PropertyType;
                    return property.GetValue(target, new object[0]);
                }
            }
            return default;
        }

        /// <inheritdoc/>
        public object VisitMember(NameExpression node)
        {
            var name = node.Name;
            var variable = scope.Current.Find(name, out object value);
            if (variable.Equals(LocalVariable.Empty) == false)
            {
                if (variable.Value.Type == null)
                    throw new Exception(string.Concat("Use of undeclared variable ", variable));
                node.Type = variable.Value.Type;
                return value;
            }
            //find in the class level
            var member = Instance.GetType().GetMember(name).FirstOrDefault();
            if (member != null)
            {
                if (member.MemberType == System.Reflection.MemberTypes.Field)
                {
                    var field = (System.Reflection.FieldInfo)member;
                    if (field.FieldType == null)
                        throw new System.Exception(string.Concat("Use of undeclared field ", field));
                    node.Type = field.FieldType;
                    return (IFSObject)field.GetValue(Instance);
                }
                if (member.MemberType == System.Reflection.MemberTypes.Property)
                {
                    var property = (System.Reflection.PropertyInfo)member;
                    node.Type = property.PropertyType;
                    return (IFSObject)property.GetValue(Instance, new object[0]);
                }
            }
            return default;
        }

        /// <inheritdoc/>
        public object VisitTernary(TernaryExpression node)
        {
            var condition = node.First.Accept(this);
            switch (condition)
            {
                case Boolean _:
                    if ((Boolean)condition)
                        return node.Second.Accept(this);
                    return node.Third.Accept(this);
                case null:
                    return node.Third.Accept(this);
                default:
                    return node.Second.Accept(this);
            }
        }

        /// <inheritdoc/>
        public object VisitThis(ThisExpression node)
        {
            return Instance;
        }

        /// <inheritdoc/>
        public object VisitUnary(UnaryExpression node)
        {
            var value = node.Operand.Accept(this);
            Type type = value.GetType();
            string name = null;
            bool updated = false;
            bool modified = false;
            switch (node.NodeType)
            {
                case ExpressionType.Parenthesized:
                    return value;
                case ExpressionType.PostfixPlusPlus:
                    name = "op_Increment";
                    updated = true;
                    break;
                case ExpressionType.PrefixPlusPlus:
                    name = "op_Increment";
                    updated = modified = true;
                    break;
                case ExpressionType.PostfixMinusMinus:
                    name = "op_Decrement";
                    updated = true;
                    break;
                case ExpressionType.PrefixMinusMinus:
                    name = "op_Decrement";
                    updated = modified = true;
                    break;
            }
            var method = Reflection.TypeUtils.GetOperatorOverload(name, out Reflection.Emit.Conversion[] _, type);
            var obj = method.Invoke(null, new object[] { value });
            if (updated)
            {
                var exp = new AssignmentExpression(node.Operand, new LiteralExpression(obj));
                exp.Accept(this);
                return modified ? obj : value;
            }
            node.Type = method.ReturnType;
            return value;
        }

        /// <inheritdoc/>
        public void VisitExpression(ExpressionStatement node)
        {
            var value = node.Expression.Accept(this);
            LongJump = () => value;
        }

        /// <inheritdoc/>
        public object VisitDeclaration(VariableDeclarationExpression node)
        {
            object value = null;
            Type type;
            if (node.Value != null)
            {
                value = node.Value.Accept(this);
                type = value.GetType();
            }
            else
            {
                type = node.VariableType.GetType(this);
            }
            scope.Create(node.Name, type, value);
            return value;
        }

        ///<inheritdoc/>
        public Type GetType(Reflection.TypeName typeName)
        {
            if (Reflection.TypeUtils.TryGetType(typeName, out Type type))
                return type;
            return Type.GetType(typeName.FullName);
        }

        /// <inheritdoc/>
        public void VisitReturn(ReturnOrThrowStatement node)
        {
            hasReturn = true;
            var value = node.Expression?.Accept(this);
            LongJump = () => value;
        }

        /// <inheritdoc/>
        public void VisitBlock(BlockStatement node)
        {
            using (var context = scope.CreateContext())
            {
                foreach (var statement in node.Statements)
                {
                    statement.Accept(this);
                    if (hasReturn)
                        break;
                }
            }
        }

        /// <inheritdoc/>
        public object VisitNull(NullExpression node)
        {
            return null;
        }

        /// <inheritdoc/>
        public object VisitNullPropegator(NullPropegatorExpression node)
        {
            object value = node.Left.Accept(this);
            if (value is null)
            {
                value = node.Right.Accept(this);
            }
            node.Type = value.GetType();
            return value;
        }

        /// <inheritdoc/>
        public void VisitDeclaration(LocalDeclarationStatement node)
        {
            foreach (var item in node.DeclarationExpressions)
            {
                item.Accept(this);
            }
        }

        /// <inheritdoc/>
        public void VisitLoop(LoopStatement node)
        {
            //todo if value has implic converter
            var statement = node.Statement;
            if (node.NodeType == StatementType.For)
            {
                using (var context = scope.CreateContext())
                {
                    for (node.Expressions[0].Accept(this); Convert.ToBoolean(node.Expressions[1].Accept(this)); node.Expressions[2].Accept(this))
                    {
                        statement.Accept(this);
                    }
                }
            }
            else if (node.NodeType == StatementType.While)
            {
                using (var context = scope.CreateContext())
                {
                    while (Convert.ToBoolean(node.Expressions[1].Accept(this)))
                    {
                        statement.Accept(this);
                    }
                }
            }
            else if (node.NodeType == StatementType.DoWhile)
            {
                using (var context = scope.CreateContext())
                {
                    do
                    {
                        statement.Accept(this);
                    } while (Convert.ToBoolean(node.Expressions[1].Accept(this)));
                }
            }
        }

        ///<inheritdoc/>
        public void VisitIf(IfStatement node)
        {
            if (Convert.ToBoolean(node.Condition.Accept(this)))
            {
                node.Then.Accept(this);
            }
            else
            {
                node.Other.Accept(this);
            }
        }
    }
}
