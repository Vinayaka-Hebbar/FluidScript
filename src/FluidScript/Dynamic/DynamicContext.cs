using FluidScript.Compiler.SyntaxTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace FluidScript.Dynamic
{
    /// <summary>
    /// Runtime evaluation of Syntax tree 
    /// <list type="bullet">it will be not same as compiled </list>
    /// </summary>
    [Serializable]
    public sealed class DynamicContext : IDictionary<string, object>, Compiler.IExpressionVisitor<object>, Compiler.IStatementVisitor, Reflection.Emit.ITypeProvider, System.Dynamic.IDynamicMetaObjectProvider, System.Runtime.Serialization.ISerializable
    {
        private readonly LocalScope scope;

        private bool hasReturn;

        private bool hasBreak;

        private bool hasContinue;

        private readonly object Instance;

        private Func<object> LongJump;

        ICollection<string> IDictionary<string, object>.Keys => scope.Keys();

        ICollection<object> IDictionary<string, object>.Values => scope.Values();

        int ICollection<KeyValuePair<string, object>>.Count => scope.Count();

        bool ICollection<KeyValuePair<string, object>>.IsReadOnly => true;

        object IDictionary<string, object>.this[string key]
        {
            get => scope[key];
            set => scope[key] = value;
        }

        /// <summary>
        /// New runtime evaluation 
        /// </summary>
        public DynamicContext(object instance)
        {
            Instance = instance;
            scope = new LocalScope();
        }

        private DynamicContext(object instance, LocalScope scope)
        {
            Instance = instance;
            this.scope = scope;
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
        /// Evaluate the <paramref name="statement"/>
        /// </summary>
        /// <exception cref="IndexOutOfRangeException"/>
        /// <exception cref="NullReferenceException"/>
        /// <exception cref="MissingMethodException"/>
        public object Invoke(Statement statement)
        {
            statement.Accept(this);
            if (LongJump != null)
                return LongJump();
            return null;
        }

        /// <summary>
        /// Evaluate the <paramref name="syntaxTree"/>
        /// </summary>
        /// <exception cref="IndexOutOfRangeException"/>
        /// <exception cref="NullReferenceException"/>
        /// <exception cref="MissingMethodException"/>
        public object Invoke(Expression syntaxTree)
        {
            return syntaxTree.Accept(this);
        }

        /// <summary>
        /// New Sub Context
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public DynamicContext CreateContext(object obj)
        {
            var scope = new LocalScope(this.scope);
            scope.CreateContext();
            foreach (var member in obj.GetType().GetMembers(Reflection.TypeUtils.DeclaredPublic))
            {
                string name = member.Name;
                object value = null;
                Type type = null;
                if (member.IsDefined(typeof(System.Diagnostics.DebuggerHiddenAttribute), false))
                    continue;
                if (member.IsDefined(typeof(Runtime.RegisterAttribute), false))
                {
                    var data = (Attribute)member.GetCustomAttributes(typeof(Runtime.RegisterAttribute), false).FirstOrDefault();
                    name = data.ToString();
                }
                switch (member)
                {
                    case System.Reflection.FieldInfo _field:
                        value = _field.GetValue(obj);
                        type = _field.FieldType;
                        break;
                    case System.Reflection.PropertyInfo _prop:
                        value = _prop.GetValue(obj, new object[0]);
                        type = _prop.PropertyType;
                        break;
                    case System.Reflection.MethodInfo _method:
                        //not get_ set_
                        if (_method.IsSpecialName) continue;
                        value = (Func<object[], object>)((args) => _method.Invoke(obj, args));
                        type = _method.ReturnType;
                        break;
                }
                scope.Create(name, type, value);
            }
            return new DynamicContext(Instance, scope);
        }

        #region Visitors
        /// <inheritdoc/>
        object Compiler.IExpressionVisitor<object>.VisitArrayLiteral(ArrayLiteralExpression node)
        {
            Type type;
            if (node.ArrayType != null)
                type = node.ArrayType.GetType(this);
            else
                type = typeof(IFSObject);
            node.Type = typeof(Collections.List<>).MakeGenericType(type);
            var items = node.Expressions;
            var length = items.Length;
            var array = (System.Collections.IList)Activator.CreateInstance(node.Type, new object[] { length });
            for (int index = 0; index < length; index++)
            {
                array.Add(items[index].Accept(this));
            }
            return array;
        }

        /// <inheritdoc/>
        object Compiler.IExpressionVisitor<object>.VisitAssignment(AssignmentExpression node)
        {
            var value = node.Right.Accept(this);
            var left = node.Left;
            string name = null;
            var instance = Instance;
            ExpressionType nodeType = left.NodeType;
            if (nodeType == ExpressionType.Identifier)
            {
                name = left.ToString();
                if (scope.TryGetMember(name, out LocalVariable variable))
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
                    var args = exp.Arguments.Select(arg => Convert.ToInt32(arg.Accept(this))).ToArray();
                    var array = (System.Collections.IList)instance;
                    var first = args.First();
                    array.Insert(first, value);
                }
                else
                {
                    var args = exp.Arguments.Select(arg => arg.Accept(this)).ToList();
                    args.Add(value);
                    var types = args.Select(arg => arg.GetType()).ToArray();
                    var indexers = type
                        .FindMembers(System.Reflection.MemberTypes.Method, Reflection.TypeUtils.Any, FindExactMethod, "set_Item");
                    var indexer = Reflection.TypeUtils.BindToMethod(indexers, types, out Reflection.Emit.Conversion[] convers);
                    exp.Setter = indexer ?? throw new Exception(string.Concat("Indexer not found at ", node.ToString()));
                    for (int i = 0; i < convers.Length; i++)
                    {
                        var conv = convers[i];
                        if (conv.HasConversion)
                            args[i] = convers[i].Convert(args[i]);
                    }
                    type = indexer.ReturnType;
                    exp.Setter.Invoke(instance, args.ToArray());
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
                LocalContext context = scope.Current;
                while (context.Parent != null)
                {
                    context = context.Parent;
                }
                var variable = scope.Create(name, value.GetType());
                //create new variable
                context.Create(variable, value);
            }
            return value;
        }

        /// <inheritdoc/>
        object Compiler.IExpressionVisitor<object>.VisitBinary(BinaryExpression node)
        {

            string opName = null;
            ExpressionType nodeType = node.NodeType;
            switch (nodeType)
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
                case ExpressionType.And:
                    opName = "op_BitwiseAnd";
                    break;
                case ExpressionType.Or:
                    opName = "op_BitwiseOr";
                    break;
                case ExpressionType.Circumflex:
                    opName = "op_ExclusiveOr";
                    break;
            }
            var left = node.Left.Accept(this);
            var right = node.Right.Accept(this);
            System.Reflection.MethodInfo method = null;
            var args = new object[] { left, right };
            if (opName != null)
            {
                if (left is null)
                    throw new OperationCanceledException(string.Concat("Null value present at ", node.Left, " in execution of ", node));
                if (right is null)
                    throw new OperationCanceledException(string.Concat("Null value present at ", node.Right, " in execution of ", node));
                var leftType = left.GetType();
                var rightType = right.GetType();
                if (leftType.IsPrimitive && rightType.IsPrimitive)
                {
                    left = FSObject.Convert(left);
                    leftType = left.GetType();
                    right = FSObject.Convert(right);
                    rightType = right.GetType();
                }
                method = Reflection.TypeUtils.
                   GetOperatorOverload(opName, out Reflection.Emit.Conversion[] conversions, leftType, rightType);
                for (int i = 0; i < conversions.Length; i++)
                {
                    Reflection.Emit.Conversion conv = conversions[i];
                    if (conv.HasConversion)
                        args[i] = conv.Convert(args[i]);
                }
            }
            else if (nodeType == ExpressionType.AndAnd || nodeType == ExpressionType.OrOr)
            {
                //left is null or not found
                if (left == null)
                    left = Boolean.False;
                //right is null or not found
                if (right == null)
                    right = Boolean.False;
                var convert = Reflection.TypeUtils.GetBooleanOveraload(left.GetType());
                //No bool conversion default true object exist
                args[0] = convert == null ? left : convert.ReflectedType != Reflection.TypeUtils.BooleanType ? Boolean.True :
                    convert.Invoke(null, new object[] { left });
                convert = Reflection.TypeUtils.GetBooleanOveraload(right.GetType());
                //No bool conversion default true object exist
                args[1] = convert == null ? right : convert.ReflectedType != Reflection.TypeUtils.BooleanType ? Boolean.True :
                    method.Invoke(null, new object[] { right });
                method = nodeType == ExpressionType.AndAnd ? Reflection.Emit.Helpers.LogicalAnd : Reflection.Emit.Helpers.LogicalOr;
            }
            node.Method = method ?? throw new Exception(string.Concat("Invalid Operation at ", node.ToString()));
            node.Type = method.ReturnType;
            return method.Invoke(null, args);
        }

        /// <inheritdoc/>
        object Compiler.IExpressionVisitor<object>.VisitCall(InvocationExpression node)
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
                return m.Name.Equals(name, StringComparison.OrdinalIgnoreCase);
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
            node.Method = method ?? throw new OperationCanceledException(string.Concat("No suitable method for ", node));
            node.Type = method.ReturnType;
            return method.Invoke(instance, args);
        }

        private bool FindExactMethod(System.Reflection.MemberInfo m, object filterCriteria)
        {
            return filterCriteria.Equals(m.Name);
        }

        /// <inheritdoc/>
        object Compiler.IExpressionVisitor<object>.VisitIndex(IndexExpression node)
        {
            var target = node.Target.Accept(this);
            if (target is null)
                throw new Exception(string.Concat("Null value present at execution of ", node.Target));
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
                var indexers = type
                    .FindMembers(System.Reflection.MemberTypes.Method, Reflection.TypeUtils.Any, FindExactMethod, "get_Item");
                var indexer = Reflection.TypeUtils.BindToMethod(indexers, types, out Reflection.Emit.Conversion[] convers);
                node.Getter = indexer ?? throw new Exception(string.Concat("Indexer not found at ", node.ToString()));
                for (int i = 0; i < convers.Length; i++)
                {
                    var conv = convers[i];
                    if (conv.HasConversion)
                        args[i] = convers[i].Convert(args[i]);
                }
                type = indexer.ReturnType;
                value = node.Getter.Invoke(target, args);
            }
            node.Type = type;
            return value;
        }

        /// <inheritdoc/>
        object Compiler.IExpressionVisitor<object>.VisitLiteral(LiteralExpression node)
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
                    value = (bool)value ? Boolean.True : Boolean.False;
                    break;
            }
            node.Type = value.GetType();
            return value;
        }


        bool HasMember(System.Reflection.MemberInfo m, object filter)
        {
            if (m.IsDefined(typeof(Runtime.RegisterAttribute), false))
            {
                var data = (Attribute)m.GetCustomAttributes(typeof(Runtime.RegisterAttribute), false).FirstOrDefault();
                if (data != null)
                    return data.Match(filter);
            }
            return filter.Equals(m.Name);
        }

        /// <inheritdoc/>
        object Compiler.IExpressionVisitor<object>.VisitMember(MemberExpression node)
        {
            var target = node.Target.Accept(this);
            if (target is null)
                throw new Exception(string.Concat("Null value present at execution of ", node.Target));
            object value = null;
            var member = target.GetType()
           .FindMembers(System.Reflection.MemberTypes.Field | System.Reflection.MemberTypes.Property, Reflection.TypeUtils.Any, HasMember, node.Name)
           .FirstOrDefault();
            if (member != null)
            {
                if (member.MemberType == System.Reflection.MemberTypes.Field)
                {
                    var field = (System.Reflection.FieldInfo)member;
                    node.Type = field.FieldType;
                    value = field.GetValue(target);

                }
                else if (member.MemberType == System.Reflection.MemberTypes.Property)
                {
                    var property = (System.Reflection.PropertyInfo)member;

                    node.Type = property.PropertyType;
                    value = property.GetValue(target, new object[0]);
                }
            }
            return value;
        }

        /// <inheritdoc/>
        object Compiler.IExpressionVisitor<object>.VisitMember(NameExpression node)
        {
            var name = node.Name;
            object value = null;
            if (scope.TryGetMember(name, out LocalVariable variable))
            {
                if (variable.Type == null)
                    throw new Exception(string.Concat("Use of undeclared variable ", variable));
                node.Type = variable.Type;
                value = scope.Current.GetValue(variable);
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
                    value = (IFSObject)field.GetValue(Instance);
                }
                if (member.MemberType == System.Reflection.MemberTypes.Property)
                {
                    var property = (System.Reflection.PropertyInfo)member;
                    node.Type = property.PropertyType;
                    value = (IFSObject)property.GetValue(Instance, new object[0]);
                }
            }
            return value;
        }

        /// <inheritdoc/>
        object Compiler.IExpressionVisitor<object>.VisitTernary(TernaryExpression node)
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
        object Compiler.IExpressionVisitor<object>.VisitThis(ThisExpression node)
        {
            return Instance;
        }

        /// <inheritdoc/>
        object Compiler.IExpressionVisitor<object>.VisitUnary(UnaryExpression node)
        {
            var value = node.Operand.Accept(this);
            if (value is null)
                throw new Exception(string.Concat("Null value present at execution of ", node));
            Type type = value.GetType();
            string name = null;
            //modified a++; updated new value
            bool modified = false, updated = true;
            switch (node.NodeType)
            {
                case ExpressionType.Parenthesized:
                    return value;
                case ExpressionType.PostfixPlusPlus:
                    name = "op_Increment";
                    modified = true;
                    updated = false;
                    break;
                case ExpressionType.PrefixPlusPlus:
                    name = "op_Increment";
                    modified = true;
                    break;
                case ExpressionType.PostfixMinusMinus:
                    name = "op_Decrement";
                    modified = true;
                    updated = false;
                    break;
                case ExpressionType.PrefixMinusMinus:
                    name = "op_Decrement";
                    modified = true;
                    break;
                case ExpressionType.Bang:
                    name = "op_LogicalNot";
                    break;
                case ExpressionType.Plus:
                    name = "op_UnaryPlus";
                    break;
                case ExpressionType.Minus:
                    name = "op_UnaryNegation";
                    break;
                case ExpressionType.Circumflex:
                    name = "op_ExclusiveOr";
                    break;
                case ExpressionType.Or:
                    name = "op_BitwiseOr";
                    break;
                case ExpressionType.And:
                    name = "op_BitwiseAnd";
                    break;
            }
            // not primitive supported it should be wrapped
            if (type.IsPrimitive)
            {
                value = FSObject.Convert(value);
                type = value.GetType();
            }
            //todo conversion
            var method = Reflection.TypeUtils.GetOperatorOverload(name, out Reflection.Emit.Conversion[] conversions, type);
            foreach (var conversion in conversions)
            {
                if (conversion.HasConversion)
                    value = conversion.Convert(value);
            }
            object obj = method.Invoke(null, new object[1] { value });
            node.Type = method.ReturnType;
            if (modified)
            {
                var exp = new AssignmentExpression(node.Operand, new LiteralExpression(obj));
                exp.Accept(this);
            }
            return updated ? obj : value;
        }

        /// <inheritdoc/>
        void Compiler.IStatementVisitor.VisitExpression(ExpressionStatement node)
        {
            var value = node.Expression.Accept(this);
            LongJump = () => value;
        }

        /// <inheritdoc/>
        object Compiler.IExpressionVisitor<object>.VisitDeclaration(VariableDeclarationExpression node)
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

        /// <inheritdoc/>
        void Compiler.IStatementVisitor.VisitReturn(ReturnOrThrowStatement node)
        {
            hasReturn = true;
            var value = node.Expression?.Accept(this);
            LongJump = () => value;
        }

        /// <inheritdoc/>
        void Compiler.IStatementVisitor.VisitBlock(BlockStatement node)
        {
            hasReturn = false;
            using (var context = scope.CreateContext())
            {
                foreach (var statement in node.Statements)
                {
                    statement.Accept(this);
                    if (hasReturn)
                        break;
                }
            }
            //for block having no return
            if (!hasReturn)
                LongJump = null;
        }

        /// <inheritdoc/>
        object Compiler.IExpressionVisitor<object>.VisitNull(NullExpression node)
        {
            return null;
        }

        /// <inheritdoc/>
        object Compiler.IExpressionVisitor<object>.VisitNullPropegator(NullPropegatorExpression node)
        {
            object value = node.Left.Accept(this);
            if (value is null)
            {
                value = node.Right.Accept(this);
            }
            if (value == null)
                throw new OperationCanceledException(string.Concat("Null Exception in ", node));
            node.Type = value.GetType();
            return value;
        }

        /// <inheritdoc/>
        object Compiler.IExpressionVisitor<object>.VisitAnonymousObject(AnonymousObjectExpression node)
        {
            var dynamicValue = new DynamicContext(this);
            foreach (var item in node.Members)
            {
                dynamicValue[item.Name] = dynamicValue.Invoke(item.Expression);
            }
            return dynamicValue;
        }

        /// <inheritdoc/>
        void Compiler.IStatementVisitor.VisitDeclaration(LocalDeclarationStatement node)
        {
            foreach (var item in node.DeclarationExpressions)
            {
                item.Accept(this);
            }
        }

        /// <inheritdoc/>
        void Compiler.IStatementVisitor.VisitLoop(LoopStatement node)
        {
            hasBreak = hasContinue = false;
            //todo if value has implic converter
            var statement = node.Body;
            if (node.NodeType == StatementType.For)
            {
                using (var context = scope.CreateContext())
                {
                    for (node.InitStatement.Accept(this); Convert.ToBoolean(node.Condition.Accept(this)); node.IncrementStatement.Accept(this))
                    {
                        statement.Accept(this);
                        if (hasBreak)
                            break;
                        if (hasContinue)
                        {
                            hasContinue = false;
                            continue;
                        }
                    }
                }
            }
            else if (node.NodeType == StatementType.While)
            {
                using (var context = scope.CreateContext())
                {
                    while (Convert.ToBoolean(node.Condition.Accept(this)))
                    {
                        statement.Accept(this);
                        if (hasBreak)
                            break;
                        if (hasContinue)
                        {
                            hasContinue = false;
                            continue;
                        }
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
                        if (hasBreak)
                            break;
                        if (hasContinue)
                        {
                            hasContinue = false;
                            continue;
                        }
                    } while (Convert.ToBoolean(node.Condition.Accept(this)));
                }
            }
        }

        ///<inheritdoc/>
        void Compiler.IStatementVisitor.VisitIf(IfStatement node)
        {
            if (Convert.ToBoolean(node.Condition.Accept(this)))
            {
                node.Then.Accept(this);
            }
            else
            {
                node.Else.Accept(this);
            }
        }

        /// <inheritdoc/>
        void Compiler.IStatementVisitor.VisitBreak(BreakStatement node)
        {
            hasBreak = true;
        }

        /// <inheritdoc/>
        void Compiler.IStatementVisitor.VisitContinue(ContinueStatement node)
        {
            hasContinue = true;
        }

        #endregion

        #region IDictionary
        bool IDictionary<string, object>.ContainsKey(string key)
        {
            return scope.Contains(key);
        }

        void IDictionary<string, object>.Add(string key, object value)
        {
            scope.CreateOrModify(key, value);
        }

        bool IDictionary<string, object>.Remove(string key)
        {
            return scope.Remove(key);
        }

        bool IDictionary<string, object>.TryGetValue(string key, out object value)
        {
            if (scope.TryGetMember(key, out LocalVariable variable))
            {
                value = scope.Current.GetValue(variable);
                return true;
            }
            value = null;
            return false;
        }

        void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item)
        {
            scope.CreateOrModify(item.Key, item.Value);
        }

        void ICollection<KeyValuePair<string, object>>.Clear()
        {
            scope.Clear();
        }

        bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item)
        {
            return scope.Contains(item.Key);
        }

        void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            foreach (KeyValuePair<string, object> item in this)
            {
                array[arrayIndex++] = item;
            }
        }

        bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
        {
            return scope.Remove(item.Key);
        }

        private static IEnumerator<KeyValuePair<string, object>> GetEnumerator(LocalScope scope)
        {
            foreach (LocalVariable item in scope)
            {
                if (scope.Current.TryGetValue(item, out object value))
                {
                    yield return new KeyValuePair<string, object>(item.Name, value);
                }
            }
        }

        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
        {
            return GetEnumerator(scope);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator(scope);
        }
        #endregion

        #region MetadataProvider
        System.Dynamic.DynamicMetaObject System.Dynamic.IDynamicMetaObjectProvider.GetMetaObject(System.Linq.Expressions.Expression parameter)
        {
            return new MetaObject(parameter, System.Dynamic.BindingRestrictions.Empty, scope);
        }
        #endregion

        #region TypeProvider

        Type Reflection.Emit.ITypeProvider.GetType(Reflection.TypeName typeName)
        {
            if (Reflection.TypeUtils.TryGetType(typeName, out Type type))
                return type;
            return Type.GetType(typeName.FullName);
        }
        #endregion

        #region ISerializable

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            var current = scope.Current;
            foreach (var item in scope)
            {
                var value = current.GetValue(item);
                if (item.Type.IsPrimitive)
                    info.AddValue(item.Name, current.GetValue(item), item.Type);
                else if (value is IConvertible convertible)
                {
                    info.AddValue(item.Name, Convert.ChangeType(value, convertible.GetTypeCode()), item.Type);
                }
                else if(value is String)
                {
                    info.AddValue(item.Name, value.ToString(), typeof(string));
                }
                else
                {
                    info.AddValue(item.Name, value, item.Type);
                }
            }
        }
        #endregion

    }
}
