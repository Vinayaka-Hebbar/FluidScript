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

        private readonly BranchContext context = new BranchContext();

        private readonly DynamicLocals locals;

        public int Count => locals.Count;

        /// <summary>
        /// New runtime evaluation with <see cref="GlobalObject"/>
        /// </summary>
        public DynamicCompiler()
        {
            target = GlobalObject.Instance;
            locals = new DynamicLocals();
        }

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
            this.target = target ?? throw new ArgumentNullException(nameof(target));
            locals = new DynamicLocals();
        }

        public DynamicCompiler(object target, IDictionary<string, object> locals)
        {
            this.target = target ?? throw new ArgumentNullException(nameof(target));
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

        private static System.Reflection.FieldInfo m_targetField;
        internal static System.Reflection.FieldInfo TargetField
        {
            get
            {
                if (m_targetField == null)
                    m_targetField = typeof(DynamicCompiler).GetField(nameof(target), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                return m_targetField;
            }
        }

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
            context.Reset();
            // if invoking expression statement result should be returned
            if (statement.NodeType == StatementType.Expression)
            {
                return ((ExpressionStatement)statement).Expression.Accept(this);
            }
            statement.Accept(this);
            return context.ReturnValue;
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
                    case System.Reflection.FieldInfo field:
                        value = field.GetValue(null);
                        type = field.FieldType;
                        break;
                    case System.Reflection.PropertyInfo _prop:
                        value = _prop.GetValue(null, new object[0]);
                        type = _prop.PropertyType;
                        break;
                    case System.Reflection.MethodInfo method:
                        //not get_ set_
                        if (method.IsSpecialName) continue;
                        var refer = (Func<object, object[], object>)method.Invoke;
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
            object obj = target;
            Binders.IBinder binder = null;
            ExpressionType nodeType = left.NodeType;
            Type type = node.Right.Type;
            if (nodeType == ExpressionType.Identifier)
            {
                var exp = (NameExpression)left;
                name = exp.Name;
                if (locals.TryLookVariable(name, out LocalVariable variable))
                {
                    locals.Update(variable, value);
                    node.Type = variable.Type;
                    return value;
                }
                else if (exp.Binder is null)
                {
                    binder = TypeUtils.GetMember(target.GetType(), name);
                    exp.Binder = binder;
                }
                if (binder is null)
                {
                    //not found, add to global
                    locals.InsertAtRoot(name, node.Right.Type, value);
                    node.Type = type;
                    return value;
                }
            }
            else if (nodeType == ExpressionType.MemberAccess)
            {
                var exp = (MemberExpression)left;
                obj = exp.Target.Accept(this);
                name = exp.Name;
                if (exp.Binder is null)
                    exp.Binder = TypeUtils.GetMember(exp.Target.Type, name);
                binder = exp.Binder;
            }
            else if (nodeType == ExpressionType.Indexer)
            {
                var exp = (IndexExpression)left;
                obj = exp.Target.Accept(this);
                if (obj == null)
                    ExecutionException.ThrowNullError(exp.Target, node);
                var args = exp.Arguments.Map(arg => arg.Accept(this)).AddLast(value);
                if (exp.Setter == null)
                {
                    var indexers = obj.GetType()
                    .GetMember("set_Item", System.Reflection.MemberTypes.Method, TypeUtils.Any);
                    var indexer = TypeHelpers.BindToMethod(indexers, args, out Binders.ArgumenConversions conversions);
                    if (indexer == null)
                        ExecutionException.ThrowMissingIndexer(exp.Target.Type, "set", exp.Target, node);

                    exp.Conversions = conversions;
                    exp.Setter = indexer;
                    // ok to be node.Right.Type instead of indexer.GetParameters().Last().ParameterType
                    Binders.ArgumentConversion valueBind = conversions.At(args.Length - 1);
                    node.Type = (valueBind == null) ? node.Right.Type : valueBind.Type;
                }
                foreach (var conversion in exp.Conversions)
                {
                    if (conversion.ConversionType == Binders.ConversionType.Convert)
                    {
                        args[conversion.Index] = conversion.Invoke(args);
                    }
                    // No params
                }
                exp.Setter.Invoke(obj, args);
                return value;
            }
            if (binder == null)
            {
                if (obj is IRuntimeMetaObjectProvider runtime)
                {
                    var result = runtime.GetMetaObject().BindSetMember(name, node.Right.Type, value).Value;
                    value = result.Value;
                    node.Type = result.Type;
                    return value;
                }
                else
                {
                    ExecutionException.ThrowMissingMember(obj.GetType(), name, node.Left, node);
                }
            }
            if (binder.Type != type && TypeUtils.TryImplicitConvert(type, binder.Type, out System.Reflection.MethodInfo method))
            {
                // implicit casting
                value = method.Invoke(null, new object[] { value });
            }
            binder.Set(obj, value);
            node.Type = binder.Type;
            return value;
        }

        #region Binary

        /// <inheritdoc/>
        object IExpressionVisitor<object>.VisitBinary(BinaryExpression node)
        {
            // todo compatible for system type
            string opName = null;
            ExpressionType nodeType = node.NodeType;
            switch (nodeType)
            {
                case ExpressionType.Plus:
                    return InvokeAddition(node);
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
                    return VisitCompare(node, "op_Inequality");
                case ExpressionType.EqualEqual:
                    return VisitCompare(node, "op_Equality");
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
                    return VisitLogical(node);
                case ExpressionType.StarStar:
                    return VisitExponentiation(node);
            }
            return Invoke(node, opName);
        }

        private object InvokeAddition(BinaryExpression node)
        {
            var left = node.Left.Accept(this);
            var right = node.Right.Accept(this);
            if (Convert.GetTypeCode(left) == TypeCode.String && Convert.GetTypeCode(right) != TypeCode.String)
            {
                right = FSConvert.ToString(right);
            }
            return Invoke(node, "op_Addition", left, right);
        }

        private object Invoke(BinaryExpression node, string opName)
        {
            if (opName == null)
                ExecutionException.ThrowInvalidOp(node);
            var left = node.Left.Accept(this);
            var right = node.Right.Accept(this);
            return Invoke(node, opName, left, right);
        }

        private static object Invoke(BinaryExpression node, string opName, object left, object right)
        {
            if (left is null)
                ExecutionException.ThrowNullError(node.Left, node);
            if (right is null)
                ExecutionException.ThrowNullError(node.Right, node);
            var leftType = left.GetType();
            var rightType = right.GetType();
            if (leftType.IsPrimitive && rightType.IsPrimitive)
            {
                left = FSConvert.ToAny(left);
                leftType = left.GetType();

                right = FSConvert.ToAny(right);
                rightType = right.GetType();
            }
            if (node.Method == null)
            {
                var method = TypeUtils.
                   GetOperatorOverload(opName, out Binders.ArgumenConversions conversions, leftType, rightType);
                if (method == null)
                    ExecutionException.ThrowInvalidOp(node);
                node.Method = method;
                node.Type = method.ReturnType;
                node.Conversions = conversions;
            }
            object[] args = new object[2] { left, right };
            // null method handled
            foreach (var conversion in node.Conversions)
            {
                if (conversion.ConversionType == Binders.ConversionType.Convert)
                {
                    args[conversion.Index] = conversion.Invoke(args);
                }
                // No Params
            }
            // operator overload invoke
            return node.Method.Invoke(null, args);
        }

        private object VisitExponentiation(BinaryExpression node)
        {
            var left = node.Left.Accept(this);
            var right = node.Right.Accept(this);
            object[] args = new object[2] { left, right };
            if (node.Method == null)
            {
                var conversions = new Binders.ArgumenConversions(2);
                if (TypeHelpers.MatchesTypes(ReflectionHelpers.MathPow, args, conversions))
                {
                    node.Conversions = conversions;
                    node.Method = ReflectionHelpers.MathPow;
                    node.Type = TypeProvider.DoubleType;
                }
                else
                {
                    ExecutionException.ThrowArgumentMisMatch(node);
                }
            }
            foreach (var conversion in node.Conversions)
            {
                if (conversion.ConversionType == Binders.ConversionType.Convert)
                {
                    args[conversion.Index] = conversion.Invoke(args);
                }
            }
            return node.Method.Invoke(null, args);
        }

        private object VisitLogical(BinaryExpression node)
        {
            var left = node.Left.Accept(this);
            var right = node.Right.Accept(this);
            //left is null or not found
            if (left is null)
                left = Boolean.False;
            //right is null or not found
            if (right is null)
                right = Boolean.False;
            var convert = TypeUtils.GetBooleanOveraload(left.GetType());
            //No bool conversion default true object exist
            left = convert == null ? left : convert.ReflectedType != TypeProvider.BooleanType ? Boolean.True :
                convert.Invoke(null, new object[] { left });
            convert = TypeUtils.GetBooleanOveraload(right.GetType());
            //No bool conversion default true object exist
            right = convert == null ? right : convert.ReflectedType != TypeProvider.BooleanType ? Boolean.True :
                convert.Invoke(null, new object[] { right });
            object[] args = new object[2] { left, right };
            if (node.Method == null)
            {
                System.Reflection.MethodInfo method = node.NodeType == ExpressionType.AndAnd ? ReflectionHelpers.LogicalAnd : ReflectionHelpers.LogicalOr;
                node.Method = method;
                node.Type = method.ReturnType;
            }
            return node.Method.Invoke(null, args);
        }

        private object VisitCompare(BinaryExpression node, string opName)
        {
            var left = node.Left.Accept(this);
            var right = node.Right.Accept(this);
            if (left is null || right is null)
            {
                object value = ReflectionHelpers.IsEquals.Invoke(null, new object[2] { left, right });
                if (node.NodeType == ExpressionType.BangEqual)
                    value = ReflectionHelpers.LogicalNot.Invoke(null, new object[1] { value });
                return value;
            }
            var leftType = left.GetType();
            var rightType = right.GetType();
            if (leftType.IsPrimitive && rightType.IsPrimitive)
            {
                left = FSConvert.ToAny(left);
                leftType = left.GetType();
                right = FSConvert.ToAny(right);
                rightType = right.GetType();
            }
            if (node.Method == null)
            {
                System.Reflection.MethodInfo method = TypeUtils.
                    GetOperatorOverload(opName, out Binders.ArgumenConversions conversions, leftType, rightType);
                node.Method = method;
                node.Conversions = conversions;
                node.Type = method.ReturnType;
            }
            var args = new object[2] { left, right };
            // null method handled
            foreach (var conversion in node.Conversions)
            {
                if (conversion.ConversionType == Binders.ConversionType.Convert)
                {
                    args[conversion.Index] = conversion.Invoke(args);
                }
                // No Params
            }
            return node.Method.Invoke(null, args);
        }
        #endregion

        #region Call

        /// <inheritdoc/>
        object IExpressionVisitor<object>.VisitCall(InvocationExpression node)
        {
            object[] args = node.Arguments.Map(arg => arg.Accept(this));
            object target = node.Method == null ? ResolveCall(node, args) : CallTarget(node);
            foreach (var convertion in node.Convertions)
            {
                if (convertion.ConversionType == Binders.ConversionType.Convert)
                {
                    args[convertion.Index] = convertion.Invoke(args);
                }
                else if (convertion.ConversionType == Binders.ConversionType.ParamArray)
                {
                    args = (object[])convertion.Invoke(args);
                    break;
                }
            }
            return node.Method.Invoke(target, args);
        }

        private object CallTarget(InvocationExpression node)
        {
            object obj;
            ExpressionType nodeType = node.Target.NodeType;
            if (nodeType == ExpressionType.Identifier)
            {
                obj = ((NameExpression)node.Target).Binder.Get(this);
            }
            else if (nodeType == ExpressionType.MemberAccess)
            {
                var exp = (MemberExpression)node.Target;
                obj = exp.Target.Accept(this);

                if (exp.Binder != null)
                {
                    var value = exp.Binder.Get(obj);
                    if (value is Delegate del)
                    {
                        obj = del;
                    }
                }
            }
            else
            {
                obj = node.Target.Accept(this);
            }

            return obj;
        }

        /// <summary>
        /// Get method
        /// </summary>
        /// <returns>target to invoke</returns>
        private object ResolveCall(InvocationExpression node, object[] args)
        {
            Binders.ArgumenConversions conversions = null;
            var target = node.Target;
            System.Reflection.MethodInfo method = null;
            string name;
            ExpressionType nodeType = node.Target.NodeType;
            // invocation target
            object obj;
            // named expression
            if (nodeType == ExpressionType.Identifier)
            {
                var exp = (NameExpression)target;
                name = exp.Name;
                if (locals.TryLookVariable(name, out LocalVariable variable))
                {
                    var refer = locals[variable.Index] as Delegate;
                    if (refer is null)
                        ExecutionException.ThrowInvalidOp(node);
                    method = TypeHelpers.GetDelegateMethod(refer, args, out conversions);
                    // only static method can allowed
                    if (method == null)
                        ExecutionException.ThrowInvalidOp(node.Target, node);
                    obj = refer.Target;
                    exp.Binder = new Binders.RuntimeVariableBinder(variable, locals);
                    exp.Type = variable.Type;
                }
                else
                {
                    obj = this.target;
                    var type = obj.GetType();
                    var methods = TypeUtils.GetPublicMethods(type, name);
                    if (methods.Length == 0)
                        ExecutionException.ThrowMissingMethod(type, name, node);
                    method = TypeHelpers.BindToMethod(methods, args, out conversions);
                    exp.Binder = new Binders.FieldBinder(TargetField);
                    // exp.Type not resolved
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
                    method = TypeHelpers.BindToMethod(methods, args, out conversions);
                }
                else if (obj is IRuntimeMetaObjectProvider runtime)
                {
                    exp.Binder = runtime.GetMetaObject().BindGetMember(name);
                    var value = exp.Binder.Get(obj);
                    if (value is Delegate del)
                    {
                        conversions = new Binders.ArgumenConversions();
                        name = nameof(Action.Invoke);
                        method = del.GetType().GetMethod(name);
                        if (TypeHelpers.MatchesTypes(method, args, conversions))
                        {
                            obj = del;
                        }
                    }
                }
                else
                {
                    ExecutionException.ThrowMissingMethod(exp.Target.Type, name, node);
                }
                // exp.Type not resolved
            }
            else
            {
                //lamda parameterized invoke
                var res = target.Accept(this);
                if (res == null)
                    ExecutionException.ThrowNullError(target, node);
                if (!(res is Delegate))
                    ExecutionException.ThrowInvalidOp(target, node);
                name = nameof(Action.Invoke);
                //Multi Delegate Invoke()
                System.Reflection.MethodInfo invoke = res.GetType().GetMethod(name);
                conversions = new Binders.ArgumenConversions();
                if (!TypeHelpers.MatchesTypes(invoke, args, conversions))
                    ExecutionException.ThrowArgumentMisMatch(node.Target, node);
                method = invoke;
                obj = res;
            }

            if (method == null)
                ExecutionException.ThrowMissingMethod(obj.GetType(), name, node);
            node.Method = method;
            node.Type = method.ReturnType;
            node.Convertions = conversions;
            return obj;
        }
        #endregion

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
            var args = node.Arguments.Map(arg => arg.Accept(this));
            if (node.Getter == null)
                ResolveIndexer(node, args);
            foreach (var conversion in node.Conversions)
            {
                if (conversion.ConversionType == Binders.ConversionType.Convert)
                {
                    args[conversion.Index] = conversion.Invoke(args);
                }
                // No params array
            }
            return node.Getter.Invoke(obj, args);
        }

        private static void ResolveIndexer(IndexExpression node, object[] args)
        {
            System.Reflection.MethodInfo indexer;
            var type = node.Target.Type;
            Binders.ArgumenConversions conversions;
            if (type.IsArray)
            {
                indexer = ReflectionHelpers.List_GetItem;
                conversions = new Binders.ArgumenConversions();
                if (TypeHelpers.MatchesTypes(indexer, args, conversions))
                    ExecutionException.ThrowMissingIndexer(type, "get", node);
            }
            else
            {
                var indexers = type
                    .GetMember("get_Item", System.Reflection.MemberTypes.Method, TypeUtils.Any);
                indexer = TypeHelpers.BindToMethod(indexers, args, out conversions);
                if (indexer == null)
                    ExecutionException.ThrowMissingIndexer(type, "get", node);
            }
            node.Conversions = conversions;
            node.Getter = indexer;
            node.Type = indexer.ReturnType;
        }

        /// <inheritdoc/>
        object IExpressionVisitor<object>.VisitLiteral(LiteralExpression node)
        {
            return node.ReflectedValue;
        }

        /// <inheritdoc/>
        object IExpressionVisitor<object>.VisitMember(MemberExpression node)
        {
            var target = node.Target.Accept(this);
            if (node.Binder == null)
            {
                var binder = TypeUtils.GetMember(node.Target.Type, node.Name);
                if (binder is null)
                {
                    if (target is IRuntimeMetaObjectProvider runtime)
                    {
                        binder = runtime.GetMetaObject().BindGetMember(node.Name);
                    }
                    else if (target is null)
                    {
                        // target null member cannot be invoked
                        ExecutionException.ThrowNullError(node);
                    }
                    else
                    {
                        node.Type = TypeProvider.ObjectType;
                        return null;
                    }
                }
                node.Binder = binder;
                node.Type = binder.Type;
            }
            return node.Binder.Get(target);
        }

        /// <inheritdoc/>
        object IExpressionVisitor<object>.VisitMember(NameExpression node)
        {
            string name = node.Name;
            object obj = target;
            Binders.IBinder binder;
            if (node.Binder == null)
            {
                if (locals.TryLookVariable(name, out LocalVariable variable))
                {
                    if (variable.Type == null)
                        throw new Exception("value not initalized");
                    binder = new Binders.RuntimeVariableBinder(variable, locals);
                }
                else
                {
                    //find in the class level
                    binder = TypeUtils.GetMember(obj.GetType(), name);
                    if (binder is null)
                    {
                        node.Type = TypeProvider.ObjectType;
                        return null;
                    }
                }
                node.Binder = binder;
                node.Type = binder.Type;
            }
            return node.Binder.Get(obj);
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
            object value;
            if (result)
            {
                value = node.Second.Accept(this);
                node.Type = node.Second.Type;
            }
            else
            {
                value = node.Third.Accept(this);
                node.Type = node.Third.Type;
            }
            return value;
        }

        /// <inheritdoc/>
        object IExpressionVisitor<object>.VisitThis(ThisExpression node)
        {
            node.Type = target.GetType();
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
                    node.Type = node.Operand.Type;
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
            Type type = node.Operand.Type;
            // no primitive supported it should be wrapped
            if (type.IsPrimitive)
            {
                value = FSConvert.ToAny(value);
                type = value.GetType();
            }
            var args = new object[1] { value };
            //resolve call
            if (node.Method == null)
            {
                var method = TypeUtils.GetOperatorOverload(name, out Binders.ArgumenConversions conversions, type);
                if (method == null)
                    ExecutionException.ThrowInvalidOp(node);
                node.Conversions = conversions;
                node.Method = method;
                node.Type = method.ReturnType;
            }
            foreach (var conversion in node.Conversions)
            {
                if (conversion.ConversionType == Binders.ConversionType.Convert)
                    value = conversion.Invoke(args);
                // no param array supported
            }
            object obj = node.Method.Invoke(null, args);
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
            node.Expression.Accept(this);
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
            return node.Compile(target.GetType(), this);
        }

        /// <inheritdoc/>
        void IStatementVisitor.VisitReturn(ReturnOrThrowStatement node)
        {
            var value = node.Expression?.Accept(this);
            if (node.NodeType == StatementType.Return)
            {
                context.Return(value);
                return;
            }
            throw new Exception(value == null ? string.Empty : value.ToString());
        }

        /// <inheritdoc/>
        void IStatementVisitor.VisitBlock(BlockStatement node)
        {
            using (locals.EnterScope())
            {
                foreach (var statement in node.Statements)
                {
                    statement.Accept(this);
                    if (context.IsJumped)
                        break;
                }
            }
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
            node.Type = typeof(DynamicObject);
            return obj;
        }

        /// <inheritdoc/>
        object IExpressionVisitor<object>.VisitSizeOf(SizeOfExpression node)
        {
            var value = node.Value.Accept(this);
            if (value == null)
                return 0;
            return System.Runtime.InteropServices.Marshal.SizeOf(value);
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
            var branch = context;
            using (context.EnterBranch())
            {
                //todo if value has implic converter
                var statement = node.Body;
                if (node.NodeType == StatementType.For)
                {
                    using (locals.EnterScope())
                    {
                        for (
                            node.Initialization.Accept(this);
                            Convert.ToBoolean(node.Condition.Accept(this));
                            node.Increments.ForEach(e => e.Accept(this))
                            )
                        {
                            statement.Accept(this);
                            if (branch.IsBreak)
                                break;
                            if (branch.IsContinue)
                            {
                                // disable for next iteration
                                branch.IsContinue = false;
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
                            if (branch.IsBreak)
                                break;
                            if (branch.IsContinue)
                            {
                                // disable for next iteration
                                branch.IsContinue = false;
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
                            if (branch.IsBreak)
                                break;
                            if (branch.IsContinue)
                            {
                                // disable for next iteration
                                branch.IsContinue = false;
                                continue;
                            }
                        } while (Convert.ToBoolean(node.Condition.Accept(this)));
                    }
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
            context.Break();
        }

        /// <inheritdoc/>
        void IStatementVisitor.VisitContinue(ContinueStatement node)
        {
            context.Continue();
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

        #region Branch Context

        struct BranchInfo
        {
            internal bool HasBreak;
            internal bool HasContinue;

            internal void Clear()
            {
                HasContinue = HasBreak = false;
            }
        }

        readonly struct ScopedBranch : IDisposable
        {
            readonly BranchContext context;
            readonly BranchInfo previous;

            public ScopedBranch(BranchContext context)
            {
                this.context = context;
                previous = context.Info;
                context.Info = new BranchInfo();
            }

            public void Dispose()
            {
                context.Info = previous;
            }
        }

        /// <summary>
        /// for long jump or break and continue
        /// </summary>
        class BranchContext
        {
            internal BranchInfo Info;

            bool HasReturn;

            internal bool IsContinue
            {
                get
                {
                    return Info.HasContinue;
                }
                set
                {
                    Info.HasContinue = value;
                }
            }

            internal object ReturnValue;

            internal bool IsBreak => HasReturn || Info.HasBreak;

            internal bool IsJumped => HasReturn || Info.HasBreak || Info.HasContinue;

            internal BranchContext()
            {
                Info = new BranchInfo();
            }

            internal void Break()
            {
                Info.HasBreak = true;
            }

            internal void Continue()
            {
                Info.HasContinue = true;
            }

            internal void Return(object value)
            {
                ReturnValue = value;
                HasReturn = true;
            }

            internal IDisposable EnterBranch()
            {
                return new ScopedBranch(this);
            }

            internal void Reset()
            {
                ReturnValue = null;
                HasReturn = false;
                Info.Clear();
            }
        }
        #endregion
    }
}
