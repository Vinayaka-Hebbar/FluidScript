using FluidScript.Compiler.SyntaxTree;
using System;
using System.Linq;

namespace FluidScript.Reflection.Emit
{
    public sealed class RuntimeContext : Compiler.IExpressionVisitor<object>, Compiler.IStatementVisitor, ITypeProvider
    {
        private readonly LocalScope scope;

        private LocalContext current;

        public readonly Node SyntaxTree;

        internal bool hasReturn;

        internal readonly object Instance;

        public Func<object> LongJump;

        public RuntimeContext(Node syntaxTree, object instance)
        {
            Instance = instance;
            scope = new LocalScope();
            current = new LocalContext(scope);
            SyntaxTree = syntaxTree;
        }

        public object this[string name]
        {
            get => current.Retrieve(name);
            set => current.CreateOrModify(name, value);
        }

        public object Invoke()
        {
            switch (SyntaxTree)
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

        public object VisitArrayLiteral(ArrayLiteralExpression node)
        {
            Type type;
            if (node.ArrayType != null)
                type = node.ArrayType.GetType(this);
            else
                type = typeof(object);
            node.Type = typeof(System.Collections.Generic.List<>).MakeGenericType(type);
            var items = node.Expressions;
            var array = (System.Collections.IList)Activator.CreateInstance(node.Type, new object[] { });
            for (int index = 0; index < items.Length; index++)
            {
                array.Add(items[index].Accept(this));
            }
            return array;
        }

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
                var variable = current.Find(name, out object _);
                if (variable.Equals(LocalVariable.Empty) == false)
                {
                    current.CreateOrModify(name, variable);
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
                current.Create(name, value.GetType(), value);
            }
            return value;
        }

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
            var method = TypeUtils.GetOperatorOverload(opName, out Conversion[] conversion, left.GetType(), right.GetType());
            node.Method = method;
            var args = new object[] { left, right };
            for (int i = 0; i < conversion.Length; i++)
            {
                Conversion conv = conversion[i];
                if (conv.HasConversion)
                    args[i] = conv.Method.Invoke(null, new object[] { args[i] });
            }
            return method.Invoke(null, args);
        }

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
                .GetMethods(TypeUtils.Any)
                .Where(HasMethod).ToArray();

            if (methods.Length == 0)
                throw new Exception(string.Concat("method ", name, " not found"));
            var method = TypeUtils.BindToMethod(methods, types, out Conversion[] conversion);
            for (int i = 0; i < conversion.Length; i++)
            {
                Conversion conv = conversion[i];
                if (conv.HasConversion)
                    args[i] = conv.Method.Invoke(null, new object[] { args[i] });
            }
            node.Method = method;
            node.Type = method.ReturnType;
            return method.Invoke(instance, args);
        }

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

        public object VisitMember(MemberExpression node)
        {
            var target = node.Target.Accept(this);
            var member = target.GetType().GetMember(node.Name).FirstOrDefault();
            if (member != null)
            {
                if (member.MemberType == System.Reflection.MemberTypes.Field)
                {
                    var field = (System.Reflection.FieldInfo)member;
                    node.Type = field.FieldType;
                    return field.GetValue(Instance);

                }
                else if (member.MemberType == System.Reflection.MemberTypes.Property)
                {
                    var property = (System.Reflection.PropertyInfo)member;

                    node.Type = property.PropertyType;
                    return property.GetValue(Instance, new object[0]);
                }
            }
            return default;
        }

        public object VisitMember(NameExpression node)
        {
            var name = node.Name;
            var variable = current.Find(name, out object value);
            if (variable != null)
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

        public object VisitTernary(TernaryExpression node)
        {
            var condition = node.First.Accept(this);
            if (condition is Boolean)
            {
                if ((Boolean)condition)
                    return node.Second.Accept(this);
                return node.Third.Accept(this);
            }
            throw new Exception("expected bool type");
        }

        public object VisitThis(ThisExpression node)
        {
            return Instance;
        }

        public object VisitUnary(UnaryExpression node)
        {
            var value = node.Operand.Accept(this);
            System.Reflection.MethodInfo method = null;
            switch (node.NodeType)
            {
                case ExpressionType.Parenthesized:
                    return value;


            }
            return value;
        }

        public void VisitExpression(ExpressionStatement node)
        {
            var value = node.Expression.Accept(this);
            LongJump = () => value;
        }

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
            current.Create(node.Name, type, value);
            return value;
        }

        public Type GetType(TypeName typeName)
        {
            if (TypeUtils.TryGetType(typeName, out Type type))
                return type;
            return Type.GetType(typeName.FullName);
        }

        public void VisitReturn(ReturnOrThrowStatement node)
        {
            hasReturn = true;
            var context = current;
            LongJump = () =>
            {
                current = context;
                return node.Expression.Accept(this);
            };
        }

        public void VisitBlock(BlockStatement node)
        {
            foreach (var statement in node.Statements)
            {
                statement.Accept(this);
                if (hasReturn)
                    break;
            }
        }
    }
}
