using FluidScript.Compiler.SyntaxTree;
using FluidScript.Runtime;
using FluidScript.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FluidScript.Compiler
{
    /// <summary>
    /// Runtime evaluation of Syntax tree 
    /// <list type="bullet">it will be not same as compiled </list>
    /// </summary>
    public sealed class DynamicCompiler : IEnumerable<KeyValuePair<string, object>>, IExpressionVisitor<object>, IStatementVisitor
    {
        private readonly object target;

        private bool hasReturn;

        private bool hasBreak;

        private bool hasContinue;

        private Func<object> LongJump;

        private readonly DynamicLocals locals;

        public int Count => locals.Count;

        /// <summary>
        /// New runtime evaluation 
        /// </summary>
        public DynamicCompiler(object target)
        {
            this.target = target;
            locals = new DynamicLocals();
        }

        public DynamicCompiler(DynamicObject target)
        {
            this.target = target;
            locals = new DynamicLocals();
        }

        public DynamicCompiler(object target, IDictionary<string, object> locals)
        {
            this.target = target;
            this.locals = new DynamicLocals(locals);
        }

        /// <summary>
        /// New runtime evaluation 
        /// </summary>
        public DynamicCompiler(DynamicCompiler other)
        {
            target = other.target;
            locals = new DynamicLocals(other.locals);
        }

        public IDictionary<string, object> Locals
        {
            get
            {
                return locals;
            }
        }

        public object Target { get => target; }

        /// <summary>
        /// Gets or Sets value for execution
        /// </summary>
        /// <param name="name">Name to store</param>
        /// <returns>value stored in it</returns>
        public object this[string name]
        {
            get => locals[name];
            set => locals[name] = value;
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
        /// Evaluate the <paramref name="expression"/>
        /// </summary>
        /// <exception cref="IndexOutOfRangeException"/>
        /// <exception cref="NullReferenceException"/>
        /// <exception cref="MissingMethodException"/>
        public object Invoke(Expression expression)
        {
            return expression.Accept(this);
        }

        public void StaticImport<T>()
        {
            var objType = typeof(T);
            foreach (var member in objType.GetMembers(TypeUtils.PublicStatic))
            {
                string name = member.Name;
                object value = null;
                Type type = null;
                if (member.IsDefined(typeof(System.Diagnostics.DebuggerHiddenAttribute), false))
                    continue;
                if (member.IsDefined(typeof(RegisterAttribute), false))
                {
                    var data = (Attribute)member.GetCustomAttributes(typeof(RegisterAttribute), false).FirstOrDefault();
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
                locals.Create(name, type, value);
            }
        }

        #region Visitors
        /// <inheritdoc/>
        object IExpressionVisitor<object>.VisitArrayLiteral(ArrayLiteralExpression node)
        {
            Type type;
            if (node.ArrayType != null)
                type = node.ArrayType.GetType(TypeProvider.Default);
            else
                type = typeof(object);
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
        object IExpressionVisitor<object>.VisitAssignment(AssignmentExpression node)
        {
            var value = node.Right.Accept(this);
            var left = node.Left;
            string name = null;
            var obj = target;
            ExpressionType nodeType = left.NodeType;
            System.Reflection.MemberInfo m = null;
            Type type = null;
            if (nodeType == ExpressionType.Identifier)
            {
                name = left.ToString();
                if (locals.TryLookVariable(name, out LocalVariable variable))
                {
                    locals.Update(variable, value);
                    node.Type = variable.Type;
                    return value;
                }
                m = Utils.TypeHelpers.GetMember(obj, name);
                if (m == null)
                {
                    type = value == null ? TypeProvider.ObjectType : value.GetType();
                    locals.InsertAtRoot(name, type, value);
                    node.Type = type;
                    return value;
                }
            }
            else if (nodeType == ExpressionType.MemberAccess)
            {
                var exp = (MemberExpression)left;
                obj = exp.Target.Accept(this);
                name = exp.Name;
                m = Utils.TypeHelpers.GetMember(obj, name);
            }
            else if (nodeType == ExpressionType.Indexer)
            {
                var exp = (IndexExpression)left;
                obj = exp.Target.Accept(this);
                if (obj == null)
                    ExecutionException.ThrowNullError(exp.Target, node);
                var args = exp.Arguments.Map(arg => arg.Accept(this)).AddLast(value);
                var indexers = obj.GetType()
                    .GetMember("set_Item", System.Reflection.MemberTypes.Method, TypeUtils.Any);
                var indexer = Utils.TypeHelpers.BindToMethod(indexers, args, out Binders.ArgumentBinderList bindings);
                if (indexer == null)
                    ExecutionException.ThrowMissingIndexer(obj, "set", exp.Target, node);
                exp.Setter = indexer;
                foreach (var binding in bindings)
                {
                    if (binding.BindType == Binders.ArgumentBinder.ArgumentBindType.Convert)
                    {
                        args[binding.Index] = binding.Invoke(args);
                    }
                    // No params
                }
                type = indexer.ReturnType;
                node.Type = type;
                exp.Setter.Invoke(obj, args);
                return value;
            }
            if (m != null)
            {
                value = TypeHelpers.InvokeSet(m, obj, value, out type);
            }
            else if (obj is IRuntimeMetaObjectProvider runtime)
            {
                var result = runtime.GetMetaObject().BindSetMember(name, node.Right.Type, value).Value;
                value = result.Value;
                type = result.Type;
            }
            else
            {
                ExecutionException.ThrowMissingMember(obj, name, node.Left, node);
            }
            node.Type = type;
            return value;
        }

        /// <inheritdoc/>
        object IExpressionVisitor<object>.VisitBinary(BinaryExpression node)
        {
            // todo compatible for system type
            string opName = null;
            ExpressionType nodeType = node.NodeType;
            System.Reflection.MethodInfo method = null;
            var left = node.Left.Accept(this);
            var right = node.Right.Accept(this);
            object[] args = new object[2] { left, right };
            switch (nodeType)
            {
                case ExpressionType.Plus:
                    if (Convert.GetTypeCode(left) == TypeCode.String && Convert.GetTypeCode(right) != TypeCode.String)
                    {
                        args[1] = right = FSConvert.ToString(right);
                    }
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
                    ExecutionException.ThrowNullError(node.Left, node);
                if (right is null)
                    ExecutionException.ThrowNullError(node.Right, node);
                var leftType = left.GetType();
                var rightType = right.GetType();
                if (leftType.IsPrimitive && rightType.IsPrimitive)
                {
                    args[0] = FSConvert.ToAny(left);
                    leftType = left.GetType();
                    args[1] = FSConvert.ToAny(right);
                    rightType = right.GetType();
                }
                method = TypeUtils.
                   GetOperatorOverload(opName, out Binders.ArgumentBinderList bindings, leftType, rightType);
                // null method handled
                foreach (var binding in bindings)
                {
                    if (binding.BindType == Binders.ArgumentBinder.ArgumentBindType.Convert)
                    {
                        args[binding.Index] = binding.Invoke(args);
                    }
                    // No Params
                }
            }
            if (method == null)
                ExecutionException.ThrowInvalidOp(node);
            node.Method = method;
            node.Type = method.ReturnType;
            // operator overload invoke
            return method.Invoke(null, args);
        }

        private static System.Reflection.MethodInfo VisitLogical(ExpressionType nodeType, ref object[] args)
        {
            var first = args[0];
            //left is null or not found
            if (first == null)
            {
                first = Boolean.False;
            }
            var second = args[1];
            //right is null or not found
            if (second == null)
                second = Boolean.False;
            args = new object[2] { first, second };
            var convert = TypeUtils.GetBooleanOveraload(first.GetType());
            //No bool conversion default true object exist
            args[0] = convert == null ? first : convert.ReflectedType != TypeProvider.BooleanType ? Boolean.True :
                convert.Invoke(null, new object[] { first });
            convert = TypeUtils.GetBooleanOveraload(second.GetType());
            //No bool conversion default true object exist
            args[1] = convert == null ? second : convert.ReflectedType != TypeProvider.BooleanType ? Boolean.True :
                convert.Invoke(null, new object[] { second });
            return nodeType == ExpressionType.AndAnd ? ReflectionHelpers.LogicalAnd : ReflectionHelpers.LogicalOr;
        }

        private static System.Reflection.MethodInfo VisitCompare(string opName, ref object[] args)
        {
            object first = args[0];
            object second = args[1];
            // todo correct method binding

            if (first is null || second is null)
                return ReflectionHelpers.IsEquals;
            var leftType = first.GetType();
            var rightType = second.GetType();
            if (leftType.IsPrimitive && rightType.IsPrimitive)
            {
                first = FSConvert.ToAny(first);
                leftType = first.GetType();
                second = FSConvert.ToAny(second);
                rightType = second.GetType();
            }
            System.Reflection.MethodInfo method = TypeUtils.
                GetOperatorOverload(opName, out Binders.ArgumentBinderList bindings, leftType, rightType);
            // null method handled
            foreach (var binding in bindings)
            {
                if (binding.BindType == Binders.ArgumentBinder.ArgumentBindType.Convert)
                {
                    args[binding.Index] = binding.Invoke(args);
                }
                // No Params
            }
            return method;
        }

        /// <inheritdoc/>
        object IExpressionVisitor<object>.VisitCall(InvocationExpression node)
        {
            var target = node.Target;
            object obj = null;
            object[] args = node.Arguments.Map(arg => arg.Accept(this));
            string name = null;
            System.Reflection.MethodInfo method = null;
            ExpressionType nodeType = node.Target.NodeType;
            Binders.ArgumentBinderList bindings = null;
            if (nodeType == ExpressionType.Identifier)
            {
                name = target.ToString();
                if (locals.TryLookVariable(name, out LocalVariable variable))
                {
                    var refer = (Delegate)locals[variable.Index];
                    method = TypeHelpers.GetDelegateMethod(refer, ref args, out bindings);
                    // only static method can allowed
                    if (method == null)
                        ExecutionException.ThrowInvalidOp(node.Target, node);
                    obj = refer.Target;
                }
                else
                {
                    obj = this.target;
                    var methods = TypeHelpers.GetPublicMethods(obj, name);
                    if (methods.Length == 0)
                        ExecutionException.ThrowMissingMethod(obj, name, node);
                    method = TypeHelpers.BindToMethod(methods, args, out bindings);
                }
            }
            else if (nodeType == ExpressionType.MemberAccess)
            {
                var exp = (MemberExpression)target;
                obj = exp.Target.Accept(this);
                name = exp.Name;
                var methods = TypeUtils.GetPublicMethods(exp.Target.Type, name);
                if (methods.Length > 0)
                {
                    method = TypeHelpers.BindToMethod(methods, args, out bindings);
                }
                else if (obj is IRuntimeMetaObjectProvider runtime)
                {
                    var del = runtime.GetMetaObject().GetDelegate(name, args, out bindings);
                    obj = del.Target;
                    method = del.Method;
                }
                else
                {
                    ExecutionException.ThrowMissingMethod(obj, name, node);
                }
            }
            else
            {
                var res = target.Accept(this);
                if (res == null)
                    ExecutionException.ThrowNullError(target, node);
                if (!(res is Delegate))
                    ExecutionException.ThrowInvalidOp(target, node);
                name = "Invoke";
                //Multi Delegate Invoke()
                System.Reflection.MethodInfo invoke = res.GetType().GetMethod(name);
                bindings = new Binders.ArgumentBinderList();
                if (!TypeHelpers.MatchesTypes(invoke, args, ref bindings))
                    ExecutionException.ThrowArgumentMisMatch(node.Target, node);
                method = invoke;
                obj = res;
            }

            foreach (var binding in bindings)
            {
                if (binding.BindType == Binders.ArgumentBinder.ArgumentBindType.Convert)
                {
                    args[binding.Index] = binding.Invoke(args);
                }
                else if (binding.BindType == Binders.ArgumentBinder.ArgumentBindType.ParamArray)
                {
                    args = (object[])binding.Invoke(args);
                    break;
                }
            }
            if (method == null)
                ExecutionException.ThrowMissingMethod(obj, name, node);
            node.Method = method;
            node.Type = method.ReturnType;
            return method.Invoke(obj, args);
        }

        object IExpressionVisitor<object>.Visit(Expression node)
        {
            return node.Accept(this);
        }

        /// <inheritdoc/>
        object IExpressionVisitor<object>.VisitIndex(IndexExpression node)
        {
            var obj = node.Target.Accept(this);
            if (obj is null)
                ExecutionException.ThrowNullError(node);
            var type = obj.GetType();
            object value = null;
            var args = node.Arguments.Map(arg => arg.Accept(this));
            if (type.IsArray)
            {
                var list = (System.Collections.IList)obj;
                type = type.GetElementType();
                var first = args.Map(arg => Convert.ToInt32(arg)).FirstOrDefault();
                value = list[first];
            }
            else
            {
                var indexers = type
                    .GetMember("get_Item", System.Reflection.MemberTypes.Method, TypeUtils.Any);
                var indexer = TypeHelpers.BindToMethod(indexers, args, out Binders.ArgumentBinderList bindings);
                if (indexer == null)
                    ExecutionException.ThrowMissingIndexer(obj, "get", node);
                node.Getter = indexer;
                foreach (var binding in bindings)
                {
                    if (binding.BindType == Binders.ArgumentBinder.ArgumentBindType.Convert)
                    {
                        args[binding.Index] = binding.Invoke(args);
                    }
                    // No params array
                }
                type = indexer.ReturnType;
                value = node.Getter.Invoke(obj, args);
            }
            node.Type = type;
            return value;
        }

        /// <inheritdoc/>
        object IExpressionVisitor<object>.VisitLiteral(LiteralExpression node)
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

        /// <inheritdoc/>
        object IExpressionVisitor<object>.VisitMember(MemberExpression node)
        {
            var target = node.Target.Accept(this);
            object value = null;
            Type type = null;
            var m = TypeHelpers.GetMember(target, node.Name);
            if (m != null)
            {
                value = TypeHelpers.InvokeGet(m, target, out type);
            }
            else if (target is IRuntimeMetaObjectProvider runtime)
            {
                var result = runtime.GetMetaObject().BindGetMember(node.Name);
                if (result.HasValue)
                {
                    var data = result.Value;
                    type = data.Type;
                    value = data.Value;
                }
            }
            else if (target is null)
            {
                ExecutionException.ThrowNullError(node);
            }
            else
            {
                // return null member
                type = TypeProvider.ObjectType;
            }
            node.Type = type;
            return value;
        }

        /// <inheritdoc/>
        object IExpressionVisitor<object>.VisitMember(NameExpression node)
        {
            string name = node.Name;
            Type type;
            object value;
            if (locals.TryLookVariable(name, out LocalVariable variable))
            {
                if (variable.Type == null)
                    throw new Exception("value not initalized");
                type = variable.Type;
                value = locals[variable.Index];
            }
            else
            {
                var obj = target;
                //find in the class level
                var m = TypeHelpers.GetMember(obj, name);
                if (m == null)
                {
                    node.Type = TypeProvider.ObjectType;
                    return null;
                }
                value = TypeHelpers.InvokeGet(m, obj, out type);
            }
            node.Type = type;
            return value;
        }

        /// <inheritdoc/>
        object IExpressionVisitor<object>.VisitTernary(TernaryExpression node)
        {
            var condition = node.First.Accept(this);
            bool result = true;
            switch (condition)
            {
                case Boolean b:
                    result = b;
                    break;
                case bool _:
                    result = (bool)condition;
                    break;
                case null:
                    result = false;
                    break;
            }

            if (result)
                return node.Second.Accept(this);
            return node.Third.Accept(this);
        }

        /// <inheritdoc/>
        object IExpressionVisitor<object>.VisitThis(ThisExpression node)
        {
            return target;
        }

        /// <inheritdoc/>
        object IExpressionVisitor<object>.VisitUnary(UnaryExpression node)
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
                ExecutionException.ThrowNullError(node.Operand, node);
            Type type = value.GetType();
            // not primitive supported it should be wrapped
            if (type.IsPrimitive)
            {
                value = FSConvert.ToAny(value);
                type = value.GetType();
            }
            //todo conversion
            var method = TypeUtils.GetOperatorOverload(name, out Binders.ArgumentBinderList bindings, type);
            if (method == null)
                ExecutionException.ThrowInvalidOp(node);
            var args = new object[1] { value };
            foreach (var binding in bindings)
            {
                if (binding.BindType == Binders.ArgumentBinder.ArgumentBindType.Convert)
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
        void IStatementVisitor.VisitExpression(ExpressionStatement node)
        {
            var value = node.Expression.Accept(this);
            LongJump = () => value;
        }

        /// <inheritdoc/>
        object IExpressionVisitor<object>.VisitDeclaration(VariableDeclarationExpression node)
        {
            object value = node.Value?.Accept(this);
            Type varType = value is null ? node.VariableType?.GetType(TypeProvider.Default) ?? TypeProvider.ObjectType : value.GetType();
            locals.Create(node.Name, varType, value);
            return value;
        }

        object IExpressionVisitor<object>.VisitAnonymousFunction(AnonymousFunctionExpression node)
        {
            return node.Compile(this);
        }

        /// <inheritdoc/>
        void IStatementVisitor.VisitReturn(ReturnOrThrowStatement node)
        {
            var value = node.Expression?.Accept(this);
            if (node.NodeType == StatementType.Return)
            {
                hasReturn = true;
                LongJump = () => value;
                return;
            }
            throw new Exception(value == null ? string.Empty : value.ToString());
        }

        /// <inheritdoc/>
        void IStatementVisitor.VisitBlock(BlockStatement node)
        {
            hasReturn = false;
            using (locals.EnterScope())
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
        object IExpressionVisitor<object>.VisitNull(NullExpression node)
        {
            return null;
        }

        /// <inheritdoc/>
        object IExpressionVisitor<object>.VisitNullPropegator(NullPropegatorExpression node)
        {
            object value = node.Left.Accept(this);
            if (value is null)
            {
                value = node.Right.Accept(this);
            }
            node.Type = node.Left.Type;
            return value;
        }

        /// <inheritdoc/>
        object IExpressionVisitor<object>.VisitAnonymousObject(AnonymousObjectExpression node)
        {
            var members = node.Members;
            var obj = new DynamicObject(members.Length);
            for (int index = 0; index < members.Length; index++)
            {
                AnonymousObjectMember item = members[index];
                var value = item.Expression.Accept(this);
                obj.Add(item.Name, value);
            }
            return obj;
        }

        /// <inheritdoc/>
        void IStatementVisitor.VisitDeclaration(LocalDeclarationStatement node)
        {
            foreach (var item in node.DeclarationExpressions)
            {
                item.Accept(this);
            }
        }

        /// <inheritdoc/>
        void IStatementVisitor.VisitLoop(LoopStatement node)
        {
            hasBreak = hasContinue = false;
            //todo if value has implic converter
            var statement = node.Body;
            if (node.NodeType == StatementType.For)
            {
                using (locals.EnterScope())
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
                using (locals.EnterScope())
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
                using (locals.EnterScope())
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
        void IStatementVisitor.VisitIf(IfStatement node)
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
        void IStatementVisitor.VisitBreak(BreakStatement node)
        {
            hasBreak = true;
        }

        /// <inheritdoc/>
        void IStatementVisitor.VisitContinue(ContinueStatement node)
        {
            hasContinue = true;
        }

        #endregion

        #region IEumerable

        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
        {
            return locals.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return locals.GetEnumerator();
        }
        #endregion



    }
}
