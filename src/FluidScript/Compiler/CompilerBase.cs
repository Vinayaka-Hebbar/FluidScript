using FluidScript.Compiler.SyntaxTree;
using FluidScript.Utils;
using System;

namespace FluidScript.Compiler
{
    /// <summary>
    /// Compiler base implementation
    /// </summary>
    public abstract class CompilerBase : IExpressionVisitor<object>
    {
        protected CompilerBase()
        {
        }

        public object Visit(Expression node)
        {
            return node.Accept(this);
        }

        public abstract object Target { get; }

        public virtual object VisitAnonymousFunction(AnonymousFunctionExpression node)
        {
            return node.Compile(Target.GetType(), this);
        }

        public virtual object VisitAnonymousObject(AnonymousObjectExpression node)
        {
            var members = node.Members;
            var obj = new Runtime.DynamicObject(members.Length);
            for (int index = 0; index < members.Length; index++)
            {
                AnonymousObjectMember item = members[index];
                var value = item.Expression.Accept(this);
                obj.Add(item.Name, value);
            }
            node.Type = typeof(Runtime.DynamicObject);
            return obj;
        }

        #region Array Literal
        public virtual object VisitArrayLiteral(ArrayLiteralExpression node)
        {
            Type type = node.ArrayType != null ? node.ArrayType.GetType(TypeProvider.Default) : TypeProvider.ObjectType;
            node.Type = typeof(Collections.List<>).MakeGenericType(type);
            var items = node.Expressions;
            var length = items.Length;
            object[] args;
            if (node.Arguments != null)
            {
                args = node.Arguments.Map(arg => arg.Accept(this));
                if (node.Constructor == null)
                {
                    var methods = node.Type.GetConstructors(TypeUtils.PublicInstance);
                    var ctor = TypeHelpers.BindToMethod(methods, args, out Binders.ArgumentConversions conversions);
                    if (ctor == null)
                        ExecutionException.ThrowMissingMethod(node.Type, ".ctor", node);
                    node.Constructor = ctor;
                    node.ArgumentConversions = conversions;
                }
                node.ArgumentConversions.Invoke(ref args);
            }
            else
            {
                args = new object[1] { new Integer(length) };
                if (node.Constructor == null)
                    node.Constructor = node.Type.GetConstructor(TypeUtils.PublicInstance, null, new Type[] { TypeProvider.IntType }, null);
            }

            var array = (System.Collections.IList)node.Constructor.Invoke(System.Reflection.BindingFlags.Default, null, args, null);
            if (length > 0)
            {
                var arrayConversions = node.ArrayConversions ?? new Binders.ArgumentConversions(items.Length);
                for (int index = 0; index < length; index++)
                {
                    Expression expression = items[index];
                    var value = expression.Accept(this);
                    var conversion = arrayConversions[index];
                    if (conversion == null)
                    {
                        if (!TypeUtils.AreReferenceAssignable(type, expression.Type) && TypeUtils.TryImplicitConvert(expression.Type, type, out System.Reflection.MethodInfo implicitCall))
                        {
                            conversion = new Binders.ParamConversion(index, implicitCall);
                        }
                        arrayConversions.Insert(index, conversion);
                    }
                    if (conversion.ConversionType == Binders.ConversionType.Convert)
                    {
                        value = conversion.Invoke(value);
                    }
                    array.Add(value);
                }
                node.ArrayConversions = arrayConversions;
            }
            return array;
        }
        #endregion

        public abstract object VisitAssignment(AssignmentExpression node);

        #region Binary Visitor
        public virtual object VisitBinary(BinaryExpression node)
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

        static object Invoke(BinaryExpression node, string opName, object left, object right)
        {
            if (left is null)
                ExecutionException.ThrowNullError(node.Left, node);
            if (right is null)
                ExecutionException.ThrowNullError(node.Right, node);

            if (node.Method == null)
            {
                var leftType = left.GetType();
                var rightType = right.GetType();
                var types = new Type[2] { leftType, rightType };
                Binders.ArgumentConversions conversions = new Binders.ArgumentConversions(2);
                if (leftType.IsPrimitive || rightType.IsPrimitive)
                {
                    TypeUtils.FromSystemType(conversions, ref types);
                }
                var method = TypeUtils.
                   GetOperatorOverload(opName, conversions, types);
                if (method == null)
                    ExecutionException.ThrowInvalidOp(node);
                node.Method = method;
                node.Type = method.ReturnType;
                node.Conversions = conversions;
            }
            object[] args = new object[2] { left, right };
            node.Conversions.Invoke(ref args);
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
                var conversions = new Binders.ArgumentConversions(2);
                if (!TypeHelpers.MatchesTypes(ReflectionHelpers.MathPow, args, conversions))
                    ExecutionException.ThrowArgumentMisMatch(node);
                node.Conversions = conversions;
                node.Method = ReflectionHelpers.MathPow;
                node.Type = TypeProvider.DoubleType;
            }
            node.Conversions.Invoke(ref args);
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
            if (node.Method == null)
            {
                var leftType = left.GetType();
                var rightType = right.GetType();
                var types = new Type[2] { leftType, rightType };
                Binders.ArgumentConversions conversions = new Binders.ArgumentConversions(2);
                if (leftType.IsPrimitive || rightType.IsPrimitive)
                {
                    TypeUtils.FromSystemType(conversions, ref types);
                }
                System.Reflection.MethodInfo method = TypeUtils.
                    GetOperatorOverload(opName, conversions, types);
                node.Method = method;
                node.Conversions = conversions;
                node.Type = method.ReturnType;
            }
            // null method handled
            object[] args = new object[2] { left, right };
            node.Conversions.Invoke(ref args);
            return node.Method.Invoke(null, args);
        }
        #endregion

        public object VisitCall(InvocationExpression node)
        {
            object[] args = node.Arguments.Map(arg => arg.Accept(this));
            object target = node.Method == null ? ResolveCall(node, args) : FindTarget(node, args);
            node.Convertions.Invoke(ref args);
            return node.Method.Invoke(target, args);
        }

        protected virtual object FindTarget(InvocationExpression node, object[] args)
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

        protected abstract object ResolveCall(InvocationExpression node, object[] args);

        /// <inheritdoc/>
        public object VisitConvert(ConvertExpression node)
        {
            var value = node.Target.Accept(this);
            if (node.Type == null)
            {
                var type = node.TypeName.GetType(TypeProvider.Default);
                if (!TypeUtils.AreReferenceAssignable(type, node.Target.Type))
                {
                    if (TypeUtils.TryImplicitConvert(node.Target.Type, type, out System.Reflection.MethodInfo implicitConvert))
                    {
                        node.Method = implicitConvert;
                    }
                    else
                    {
                        ExecutionException.ThrowInvalidCast(node.Type, node);
                    }
                }

                node.Type = type;
            }
            return value;
        }

        public abstract object VisitDeclaration(VariableDeclarationExpression node);

        object IExpressionVisitor<object>.VisitIndex(IndexExpression node)
        {
            var obj = node.Target.Accept(this);
            if (obj is null)
                ExecutionException.ThrowNullError(node);
            var args = node.Arguments.Map(arg => arg.Accept(this));
            if (node.Getter == null)
                ResolveIndexer(node, args);
            node.Conversions.Invoke(ref args);
            return node.Getter.Invoke(obj, args);
        }

        static void ResolveIndexer(IndexExpression node, object[] args)
        {
            System.Reflection.MethodInfo indexer;
            var type = node.Target.Type;
            Binders.ArgumentConversions conversions;
            if (type.IsArray)
            {
                indexer = ReflectionHelpers.List_GetItem;
                conversions = new Binders.ArgumentConversions(args.Length);
                if (!TypeHelpers.MatchesTypes(indexer, args, conversions))
                    ExecutionException.ThrowMissingIndexer(type, "get", node);
                node.Type = type.GetElementType();
            }
            else
            {
                var indexers = type
                    .GetMember("get_Item", System.Reflection.MemberTypes.Method, TypeUtils.Any);
                indexer = TypeHelpers.BindToMethod(indexers, args, out conversions);
                if (indexer == null)
                    ExecutionException.ThrowMissingIndexer(type, "get", node);
                node.Type = indexer.ReturnType;
            }
            node.Conversions = conversions;
            node.Getter = indexer;
        }

        public object VisitLiteral(LiteralExpression node)
        {
            return node.ReflectedValue;
        }

        public abstract object VisitMember(MemberExpression node);

        public abstract object VisitMember(NameExpression node);

        public object VisitNull(NullExpression node)
        {
            return null;
        }

        public object VisitNullPropegator(NullPropegatorExpression node)
        {
            object value = node.Left.Accept(this);
            if (value is null)
            {
                value = node.Right.Accept(this);
            }
            node.Type = node.Left.Type;
            return value;
        }

        public virtual object VisitSizeOf(SizeOfExpression node)
        {
            var value = node.Value.Accept(this);
            if (value == null)
                return 0;
            return System.Runtime.InteropServices.Marshal.SizeOf(value);
        }

        public object VisitTernary(TernaryExpression node)
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

        public object VisitThis(ThisExpression node)
        {
            node.Type = Target.GetType();
            return Target;
        }

        #region Unary Expression
        public object VisitUnary(UnaryExpression node)
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
            // no primitive supported it should be wrapped

            //resolve call
            if (node.Method == null)
            {
                Type type = node.Operand.Type;
                Binders.ArgumentConversions conversions = new Binders.ArgumentConversions(1);
                if (type.IsPrimitive)
                {
                    var typeCode = Type.GetTypeCode(type);
                    conversions.Add(new Binders.ParamConversion(0, ReflectionHelpers.ToAny));
                    type = TypeProvider.Find(typeCode);
                    conversions.Backup();
                }
                var method = TypeUtils.GetOperatorOverload(name, conversions, type);
                if (method == null)
                    ExecutionException.ThrowInvalidOp(node);
                node.Conversions = conversions;
                node.Method = method;
                node.Type = method.ReturnType;
            }
            object[] args = new object[1] { value };
            node.Conversions.Invoke(ref args);
            object obj = node.Method.Invoke(null, args);
            if (modified)
            {
                var exp = new AssignmentExpression(node.Operand, new LiteralExpression(obj));
                exp.Accept(this);
            }
            return updated ? obj : value;
        }
        #endregion
    }
}
