using FluidScript.Compiler.SyntaxTree;
using FluidScript.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FluidScript.Dynamic
{
    /// <summary>
    /// Runtime evaluation of Syntax tree 
    /// <list type="bullet">it will be not same as compiled </list>
    /// </summary>
    [Serializable]
    public sealed class DynamicContext : ICollection<KeyValuePair<string, object>>, Compiler.IExpressionVisitor<object>, Compiler.IStatementVisitor, Reflection.Emit.ITypeProvider, System.Runtime.Serialization.ISerializable
    {
        [NonSerialized]
        internal readonly DynamicClass Class;

        internal DynamicObject Current;

        private readonly DynamicObject root;

        private bool hasReturn;

        private bool hasBreak;

        private bool hasContinue;

        private Func<object> LongJump;

        int ICollection<KeyValuePair<string, object>>.Count => Class.Count;

        bool ICollection<KeyValuePair<string, object>>.IsReadOnly => false;

        //todo instead of this import
        /// <summary>
        /// New runtime evaluation 
        /// </summary>
        public DynamicContext(object instance)
        {
            Class = new DynamicClass(instance);
            Current = root = new DynamicObject(Class);
        }

        /// <summary>
        /// New runtime evaluation 
        /// </summary>
        public DynamicContext(DynamicContext other)
        {
            Class = new DynamicClass(other.Class);
            root = other.root;
            Current = new DynamicObject(Class, other.Current);
        }


        /// <summary>
        /// Gets or Sets value
        /// </summary>
        /// <param name="name">Name to store</param>
        /// <returns>value stored in it</returns>
        public object this[string name]
        {
            get => Current[name];
            set => Current[name] = value;
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

        public void StaticImport<T>()
        {
            var objType = typeof(T);
            foreach (var member in objType.GetMembers(Utils.TypeUtils.PublicStatic))
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
                        value = _field.GetValue(null);
                        type = _field.FieldType;
                        break;
                    case System.Reflection.PropertyInfo _prop:
                        value = _prop.GetValue(null, new object[0]);
                        type = _prop.PropertyType;
                        break;
                    case System.Reflection.MethodInfo _method:
                        //not get_ set_
                        if (_method.IsSpecialName) continue;
                        var refer = (Func<object, object[], object>)_method.Invoke;
                        value = refer;
                        type = value.GetType();
                        break;
                }
                var variable = Class.Create(name, type);
                Current.Insert(variable, value, true);
            }
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
            var instance = Class.Instance;
            ExpressionType nodeType = left.NodeType;
            if (nodeType == ExpressionType.Identifier)
            {
                name = left.ToString();
                if (Class.TryLookVariable(name, out LocalVariable variable))
                {
                    Current.Modify(variable, value);
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
                if (instance == null)
                    throw new ArgumentNullException(string.Concat("Null value present at ", node, " in execution of ", exp.Target));
                var type = instance.GetType();
                var args = exp.Arguments.Select(arg => arg.Accept(this)).ToList();
                args.Add(value);
                var indexers = type
                    .FindMembers(System.Reflection.MemberTypes.Method, Utils.TypeUtils.Any, FindExactMethod, "set_Item");
                var indexer = Utils.TypeUtils.BindToMethod(indexers, args, out Reflection.Emit.ParamBindList bindings);
                exp.Setter = indexer ?? throw new Exception(string.Concat("Indexer not found at ", node.ToString()));
                foreach (var binding in bindings)
                {
                    if (binding.BindType == Reflection.Emit.ParamBind.ParamBindType.Convert)
                    {
                        args[binding.Index] = binding.Invoke(args);
                    }
                    // No params
                }
                type = indexer.ReturnType;
                exp.Setter.Invoke(instance, args.ToArray());
                return value;
            }
            if (instance == null)
                throw new Exception(string.Concat("null value at ", node.Left));
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
            else if (instance is IRuntimeMetaObjectProvider runtime)
            {
                var metaObject = runtime.GetMetaObject();
                metaObject.BindSetMember(name, node.Type, value);
            }
            else
            {
                var variable = Class.Create(name, value.GetType());
                root.Insert(variable, value, false);
            }
            return value;
        }

        /// <inheritdoc/>
        object Compiler.IExpressionVisitor<object>.VisitBinary(BinaryExpression node)
        {

            string opName = null;
            ExpressionType nodeType = node.NodeType;
            var left = node.Left.Accept(this);
            var right = node.Right.Accept(this);
            System.Reflection.MethodInfo method = null;
            var args = new object[] { left, right };
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
                    method = VisitCompare("op_Inequality", ref args);
                    break;
                case ExpressionType.EqualEqual:
                    method = VisitCompare("op_Equality", ref args);
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
                case ExpressionType.AndAnd:
                case ExpressionType.OrOr:
                    method = VisitLogical(nodeType, ref args);
                    break;
            }
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
                method = TypeUtils.
                   GetOperatorOverload(opName, out Reflection.Emit.ParamBindList bindings, leftType, rightType);
                // null method handled
                foreach (var binding in bindings)
                {
                    if (binding.BindType == Reflection.Emit.ParamBind.ParamBindType.Convert)
                    {
                        args[binding.Index] = binding.Invoke(args);
                    }
                    // No Params
                }
            }
            node.Method = method ?? throw new Exception(string.Concat("Invalid Operation at ", node.ToString()));
            node.Type = method.ReturnType;
            return method.Invoke(null, args);
        }

        private static System.Reflection.MethodInfo VisitLogical(ExpressionType nodeType, ref object[] args)
        {
            var left = args[0];
            var right = args[1];
            //left is null or not found
            if (left == null)
                left = Boolean.False;
            //right is null or not found
            if (right == null)
                right = Boolean.False;
            var convert = TypeUtils.GetBooleanOveraload(left.GetType());
            //No bool conversion default true object exist
            args[0] = convert == null ? left : convert.ReflectedType != TypeUtils.BooleanType ? Boolean.True :
                convert.Invoke(null, new object[] { left });
            convert = TypeUtils.GetBooleanOveraload(right.GetType());
            //No bool conversion default true object exist
            args[1] = convert == null ? right : convert.ReflectedType != TypeUtils.BooleanType ? Boolean.True :
                convert.Invoke(null, new object[] { right });
            return nodeType == ExpressionType.AndAnd ? Helpers.LogicalAnd : Helpers.LogicalOr;
        }

        private static System.Reflection.MethodInfo VisitCompare(string opName, ref object[] args)
        {
            object left = args[0];
            object right = args[1];
            // todo correct method binding
            if (left is null || right is null)
                return Helpers.IsEquals;
            var leftType = left.GetType();
            var rightType = right.GetType();
            if (leftType.IsPrimitive && rightType.IsPrimitive)
            {
                left = FSObject.Convert(left);
                leftType = left.GetType();
                right = FSObject.Convert(right);
                rightType = right.GetType();
            }
            System.Reflection.MethodInfo method = TypeUtils.
                GetOperatorOverload(opName, out Reflection.Emit.ParamBindList bindings, leftType, rightType);
            // null method handled
            foreach (var binding in bindings)
            {
                if (binding.BindType == Reflection.Emit.ParamBind.ParamBindType.Convert)
                {
                    args[binding.Index] = binding.Invoke(args);
                }
                // No Params
            }
            return method;
        }

        /// <inheritdoc/>
        object Compiler.IExpressionVisitor<object>.VisitCall(InvocationExpression node)
        {
            var target = node.Target;
            object obj = null;
            object[] args = node.Arguments.Select(arg => arg.Accept(this)).ToArray();
            string name = null;
            System.Reflection.MethodInfo method = null;
            ExpressionType nodeType = node.Target.NodeType;
            Reflection.Emit.ParamBindList bindings = null;
            if (nodeType == ExpressionType.Identifier)
            {
                name = target.ToString();
                LocalVariable[] variables = Class.LookVariables(name, (item) => typeof(Delegate).IsAssignableFrom(item.Type)).ToArray();
                if (variables.Length > 0)
                {
                    bindings = new Reflection.Emit.ParamBindList();
                    for (int index = 0; index < variables.Length; index++)
                    {
                        var refer = (Delegate)Current.GetValue(variables[index]);
                        System.Reflection.MethodInfo m = refer.Method;
                        // only static method can allowed
                        if (refer.Target is System.Reflection.MethodInfo)
                        {
                            m = (System.Reflection.MethodInfo)refer.Target;
                            if (TypeUtils.MatchesTypes(m, args, ref bindings))
                            {
                                method = m;
                                break;
                            }
                        }
                        else if (refer.Target is Function function)
                        {
                            if (TypeUtils.MatchesTypes(function.ParameterTypes, args, ref bindings))
                            {
                                args = new object[] { args };
                                obj = function;
                                method = m;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    obj = Class.Instance;
                    var methods = GetInstanceMethod(obj, name);

                    if (methods.Length == 0)
                        throw new Exception(string.Concat("method '", name, "' not found in execution of ", node));
                    method = TypeUtils.BindToMethod(methods, args, out bindings);
                }
            }
            else if (nodeType == ExpressionType.MemberAccess)
            {
                var exp = (MemberExpression)target;
                obj = exp.Target.Accept(this);
                name = exp.Name;
                var methods = GetInstanceMethod(obj, name);
                if (methods.Length == 0)
                {
                    if (obj is IRuntimeMetaObjectProvider runtime)
                    {
                        var metaObject = runtime.GetMetaObject();
                        return metaObject.BindInvokeMemeber(name, args);
                    }
                    throw new Exception(string.Concat("method '", name, "' not found in execution of ", node));
                }
                method = TypeUtils.BindToMethod(methods, args, out bindings);
            }

            foreach (var binding in bindings)
            {
                if (binding.BindType == Reflection.Emit.ParamBind.ParamBindType.Convert)
                {
                    args[binding.Index] = binding.Invoke(args);
                }
                else if (binding.BindType == Reflection.Emit.ParamBind.ParamBindType.ParamArray)
                {
                    args = (object[])binding.Invoke(args);
                    break;
                }
            }
            node.Method = method ?? throw new Exception(string.Concat("No suitable method for ", node));
            node.Type = method.ReturnType;
            return method.Invoke(obj, args);
        }

        private System.Reflection.MethodInfo[] GetInstanceMethod(object instance, string name)
        {
            if (instance == null)
                return new System.Reflection.MethodInfo[0];
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
            return resultType
                  .GetMethods(TypeUtils.Any)
                  .Where(HasMethod).ToArray();
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
                var list = (System.Collections.IList)target;
                if (type.IsArray)
                {
                    type = type.GetElementType();
                }
                else if (type.IsGenericType)
                {
                    type = type.GetGenericArguments()[0];
                }
                else
                {
                    type = typeof(object);
                }
                var first = args.Select(arg => Convert.ToInt32(arg)).FirstOrDefault();
                value = list[first];
            }
            else
            {
                var indexers = type
                    .FindMembers(System.Reflection.MemberTypes.Method, TypeUtils.Any, FindExactMethod, "get_Item");
                var indexer = TypeUtils.BindToMethod(indexers, args, out Reflection.Emit.ParamBindList bindings);
                node.Getter = indexer ?? throw new Exception(string.Concat("Indexer not found at ", node.ToString()));
                foreach (var binding in bindings)
                {
                    if (binding.BindType == Reflection.Emit.ParamBind.ParamBindType.Convert)
                    {
                        args[binding.Index] = binding.Invoke(args);
                    }
                    // No params array
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
           .FindMembers(System.Reflection.MemberTypes.Field | System.Reflection.MemberTypes.Property, TypeUtils.Any, HasMember, node.Name)
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
            else if (target is IRuntimeMetaObjectProvider runtime)
            {
                var metaObject = runtime.GetMetaObject();
                var result = metaObject.BindGetMember(node.Name);
                node.Type = result.Type;
                value = result.Value;
            }
            return value;
        }

        /// <inheritdoc/>
        object Compiler.IExpressionVisitor<object>.VisitMember(NameExpression node)
        {
            string name = node.Name;
            object value = null;
            if (Class.TryLookVariable(name, out LocalVariable variable))
            {
                if (variable.Type == null)
                    throw new Exception("value not initalized");
                value = Current.GetValue(variable);
            }
            else
            {
                var instance = Class.Instance;
                //find in the class level
                var member = instance.GetType().GetMember(name).FirstOrDefault();
                if (member != null)
                {
                    if (member.MemberType == System.Reflection.MemberTypes.Field)
                    {
                        var field = (System.Reflection.FieldInfo)member;
                        if (field.FieldType == null)
                            throw new Exception(string.Concat("Use of undeclared field ", field));
                        node.Type = field.FieldType;
                        value = (IFSObject)field.GetValue(instance);
                    }
                    if (member.MemberType == System.Reflection.MemberTypes.Property)
                    {
                        var property = (System.Reflection.PropertyInfo)member;
                        node.Type = property.PropertyType;
                        value = (IFSObject)property.GetValue(instance, new object[0]);
                    }
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
            return root;
        }

        /// <inheritdoc/>
        object Compiler.IExpressionVisitor<object>.VisitUnary(UnaryExpression node)
        {
            var value = node.Operand.Accept(this);
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
                    // here value is null it is as not defined
                    if (value is null)
                        return Boolean.True;
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
            if (value is null)
                throw new Exception(string.Concat("Null value present at execution of ", node));
            Type type = value.GetType();
            // not primitive supported it should be wrapped
            if (type.IsPrimitive)
            {
                value = FSObject.Convert(value);
                type = value.GetType();
            }
            //todo conversion
            var method = TypeUtils.GetOperatorOverload(name, out Reflection.Emit.ParamBindList bindings, type);
            if (method == null)
                throw new Exception(string.Concat("Invalid operation at ", node));
            var args = new object[1] { value };
            foreach (var binding in bindings)
            {
                if (binding.BindType == Reflection.Emit.ParamBind.ParamBindType.Convert)
                    value = binding.Invoke(args);
                // no param array
            }
            object obj = method.Invoke(null, args);
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
            var variable = Class.Create(node.Name, type);
            Current.Insert(variable, value, true);
            return value;
        }

        object Compiler.IExpressionVisitor<object>.VisitAnonymousFunction(AnonymousFunctionExpression node)
        {
            var parameters = node.Parameters;
            var resolvedParams = parameters.Select(arg => arg.GetParameterInfo(this)).ToArray();
            var arguments = resolvedParams.Select(arg => arg.Type).ToArray();
            object CallAnonymous(params object[] args)
            {
                using (var scoped = new ScopedContext(this))
                {
                    //todo type match
                    for (int index = 0; index < resolvedParams.Length; index++)
                    {
                        Reflection.Emit.ParameterInfo param = resolvedParams[index];
                        var variable = Class.Create(param.Name, param.Type);
                        Current.Insert(variable, args[index], true);
                    }
                    node.Body.Accept(this);
                    return LongJump?.Invoke();
                }
            }
            var function = new Function(arguments, (Func<object[], object>)CallAnonymous);
            return (Func<object[], object>)function.Invoke;
        }

        /// <inheritdoc/>
        void Compiler.IStatementVisitor.VisitReturn(ReturnOrThrowStatement node)
        {
            if (node.NodeType == StatementType.Return)
            {
                hasReturn = true;
                var value = node.Expression?.Accept(this);
                LongJump = () => value;
            }
            else
            {

            }
        }

        /// <inheritdoc/>
        void Compiler.IStatementVisitor.VisitBlock(BlockStatement node)
        {
            hasReturn = false;
            using (var context = new ScopedContext(this))
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
            var objClass = new DynamicClass(Class);
            var obj = new DynamicObject(objClass, Current);
            foreach (var item in node.Members)
            {
                var value = item.Expression.Accept(this);
                var variable = objClass.Create(item.Name, value.GetType());
                obj.Insert(variable, value, true);
            }
            return obj;
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
                using (var context = new ScopedContext(this))
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
                using (var context = new ScopedContext(this))
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
                using (var context = new ScopedContext(this))
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
            var value = node.Condition.Accept(this);
            if (Convert.ToBoolean(value))
            {
                node.Then.Accept(this);
            }
            else
            {
                node.Else?.Accept(this);
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

        #region ICollection

        void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item)
        {
            Current.Insert(item.Key, item.Value);
        }

        void ICollection<KeyValuePair<string, object>>.Clear()
        {
            Class.Clear();
        }

        bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item)
        {
            return Current.ContainsKey(item.Key);
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
            return Current.Remove(item.Key);
        }

        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
        {
            return Current.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return Current.GetEnumerator();
        }
        #endregion

        #region TypeProvider

        Type Reflection.Emit.ITypeProvider.GetType(Reflection.TypeName typeName)
        {
            if (TypeUtils.TryGetType(typeName, out Type type))
                return type;
            return Type.GetType(typeName.FullName);
        }
        #endregion

        #region ISerializable

        void System.Runtime.Serialization.ISerializable.GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            ((System.Runtime.Serialization.ISerializable)Current).GetObjectData(info, context);
        }
        #endregion



    }
}
