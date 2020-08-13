using FluidScript.Compiler.SyntaxTree;
using FluidScript.Extensions;
using FluidScript.Runtime;
using FluidScript.Utils;
using System;
using System.Linq;

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

        public abstract object Target { get; }

        TypeContext typeContext;

        public TypeContext TypeContext
        {
            get
            {
                if (typeContext == null)
                    typeContext = new TypeContext(null);
                return typeContext;
            }
        }

        object IExpressionVisitor<object>.Default(Expression node)
        {
            return null;
        }

        public virtual object VisitAnonymousFunction(AnonymousFunctionExpression node)
        {
            return node.Compile(Target.GetType(), this);
        }

        public virtual object VisitAnonymousObject(AnonymousObjectExpression node)
        {
            var members = node.Members;
            var obj = new DynamicObject(members.Count);
            for (int index = 0; index < members.Count; index++)
            {
                AnonymousObjectMember item = members[index];
                var value = item.Expression.Accept(this);
                obj.Add(item.Name, value);
            }
            node.Type = typeof(DynamicObject);
            return obj;
        }

        #region Array Literal
        public virtual object VisitArrayLiteral(ArrayListExpression node)
        {
            Type type = node.ArrayType != null ? node.ArrayType.ResolveType(TypeContext) : TypeProvider.ObjectType;
            node.Type = typeof(Collections.List<>).MakeGenericType(type);
            node.ElementType = type;
            var items = node.Expressions;
            var length = items.Count;
            object[] args;
            if (node.Arguments != null)
            {
                args = node.Arguments.Map(arg => arg.Accept(this));
                if (node.Constructor == null)
                {
                    var methods = node.Type.GetConstructors(Utils.ReflectionUtils.PublicInstance);
                    var ctor = ReflectionUtils.BindToMethod(methods, args, out ArgumentConversions conversions);
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
                    node.Constructor = node.Type.GetConstructor(ReflectionUtils.PublicInstance, null, new Type[] { TypeProvider.IntType }, null);
            }

            var array = (System.Collections.IList)node.Constructor.Invoke(System.Reflection.BindingFlags.Default, null, args, null);
            if (length > 0)
            {
                var arrayConversions = node.ArrayConversions ?? new ArgumentConversions(items.Count);
                for (int index = 0; index < length; index++)
                {
                    Expression expression = items[index];
                    var value = expression.Accept(this);
                    var conversion = arrayConversions[index];
                    if (conversion == null && !TypeUtils.AreReferenceAssignable(type, expression.Type) && expression.Type.TryImplicitConvert(type, out System.Reflection.MethodInfo implicitCall))
                    {
                        conversion = new ParamConversion(index, implicitCall);
                        arrayConversions.Append(index, conversion);
                    }
                    if (conversion != null && conversion.ConversionType == ConversionType.Normal)
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

        #region Visit Indexer

        protected object AssignIndexer(AssignmentExpression node, object value)
        {
            var exp = (IndexExpression)node.Left;
            var obj = exp.Target.Accept(this);
            if (obj == null)
                ExecutionException.ThrowNullError(exp.Target, node);
            var args = exp.Arguments.Map(arg => arg.Accept(this)).AddLast(value);
            if (exp.Setter == null)
            {
                var indexer = exp.Target.Type
                    .FindSetIndexer(args, out ArgumentConversions conversions);
                if (indexer == null)
                    ExecutionException.ThrowMissingIndexer(exp.Target.Type, "set", exp.Target, node);

                exp.Conversions = conversions;
                exp.Setter = indexer;
                // ok to be node.Right.Type instead of indexer.GetParameters().Last().ParameterType
                var valueBind = conversions[args.Length - 1];
                node.Type = (valueBind == null) ? node.Right.Type : valueBind.Type;
            }
            exp.Conversions.Invoke(ref args);
            exp.Setter.Invoke(obj, args);
            return value;
        }

        #endregion

        #region Binary Visitor
        public virtual object VisitBinary(BinaryExpression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.Plus:
                    return InvokeAddition(node);
                case ExpressionType.BangEqual:
                    return VisitCompare(node, "op_Inequality");
                case ExpressionType.EqualEqual:
                    return VisitCompare(node, "op_Equality");
                case ExpressionType.AndAnd:
                case ExpressionType.OrOr:
                    return VisitLogical(node);
                case ExpressionType.StarStar:
                    return VisitExponentiation(node);
            }
            return Invoke(node, node.MethodName);
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
                ArgumentConversions conversions = new ArgumentConversions(2);
                System.Reflection.MethodInfo method;
                if (leftType.IsPrimitive || rightType.IsPrimitive)
                {
                    var initial = (ReflectionUtils.FromSystemType(ref types));
                    method = ReflectionUtils.
                       GetOperatorOverload(opName, conversions, types);
                    conversions.SetInitial(initial);
                }
                else
                {
                    method = ReflectionUtils.
                       GetOperatorOverload(opName, conversions, types);
                }
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
            if (left is null)
                ExecutionException.ThrowNullError(node, node.Left);
            if (right is null)
                ExecutionException.ThrowNullError(node, node.Right);
            object[] args = new object[2] { left, right };
            if (node.Method == null)
            {
                Type leftType = left.GetType();
                Type rightType = right.GetType();
                var types = new Type[] { leftType, rightType };
                var conversions = new ArgumentConversions(2);
                if (leftType.IsPrimitive || rightType.IsPrimitive)
                    conversions.SetInitial(ReflectionUtils.FromSystemType(ref types));
                if (!ReflectionHelpers.MathPow.MatchesArgumentTypes(types, conversions))
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
            var convert = ReflectionUtils.GetImplicitToBooleanOperator(left.GetType());
            //No bool conversion default true object exist
            left = convert == null ? left : convert.ReflectedType != TypeProvider.BooleanType ? Boolean.True :
                convert.Invoke(null, new object[] { left });
            convert = ReflectionUtils.GetImplicitToBooleanOperator(right.GetType());
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
                return NullCompare(node, left, right);
            if (node.Method == null)
            {
                var leftType = left.GetType();
                var rightType = right.GetType();
                var types = new Type[2] { leftType, rightType };
                ArgumentConversions conversions = new ArgumentConversions(2);
                System.Reflection.MethodInfo method;
                if (leftType.IsPrimitive || rightType.IsPrimitive)
                {
                    var initial = ReflectionUtils.FromSystemType(ref types);
                    method = ReflectionUtils.
                   GetOperatorOverload(opName, conversions, types);
                    conversions.SetInitial(initial);
                }
                else
                {
                    method = ReflectionUtils.
                    GetOperatorOverload(opName, conversions, types);
                }
                if (method == null)
                    return NullCompare(node, left, right);
                node.Method = method;
                node.Conversions = conversions;
                node.Type = method.ReturnType;
            }
            // null method handled
            object[] args = new object[2] { left, right };
            node.Conversions.Invoke(ref args);
            return node.Method.Invoke(null, args);
        }

        private static object NullCompare(BinaryExpression node, object left, object right)
        {
            System.Reflection.MethodInfo isEquals = ReflectionHelpers.IsEquals;
            object value = isEquals.Invoke(null, new object[2] { left, right });
            if (node.NodeType == ExpressionType.BangEqual)
                value = ReflectionHelpers.LogicalNot.Invoke(null, new object[1] { value });
            node.Type = isEquals.ReturnType;
            return value;
        }
        #endregion

        public object VisitCall(InvocationExpression node)
        {
            object[] args = node.Arguments.Map(arg => arg.Accept(this));
            object target = node.Method == null ? ResolveCall(node, args) : FindTarget(node, args);
            node.Conversions.Invoke(ref args);
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
                var type = node.TypeName.ResolveType(TypeContext);
                if (!TypeUtils.AreReferenceAssignable(type, node.Target.Type))
                {
                    if (!node.Target.Type.TryImplicitConvert(type, out System.Reflection.MethodInfo method) &&
                        !node.Target.Type.TryExplicitConvert(type, out method))
                    {
                        ExecutionException.ThrowInvalidCast(type, node);
                    }
                    node.Method = method;
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
            indexer = type
                    .FindGetIndexer(args, out ArgumentConversions conversions);
            if (indexer == null)
                ExecutionException.ThrowMissingIndexer(type, "get", node);
            node.Type = indexer.ReturnType;
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
            bool result;
            if(condition is Boolean)
            {
                result = (Boolean)condition;
            }
            else if(condition is bool)
            {
                result = (bool)condition;
            }
            else if(condition is null)
            {
                result = false;
            }
            else
            {
                IConvertible c = condition as IConvertible;
                // null mean other type
                result = c == null || (c.GetTypeCode() != TypeCode.Boolean) || c.ToBoolean(null);
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

        public object VisitSuper(SuperExpression node)
        {
            node.Type = Target.GetType().BaseType;
            return Target;
        }

        public object VisitNew(NewExpression node)
        {
            var args = node.Arguments.Map(a => a.Accept(this));
            if (node.Constructor == null)
            {
                node.Type = node.TypeSyntax.ResolveType(TypeContext);
                node.Constructor = ReflectionUtils.BindToMethod(node.Type.GetConstructors(), args, out ArgumentConversions conversions);
                node.Conversions = conversions;
            }
            node.Conversions.Invoke(ref args);
            return node.Constructor.Invoke(args);
        }

        public object VisitInstanceOf(InstanceOfExpression node)
        {
            var value = node.Target.Accept(this);
            if (value is null && node.TypeSyntax is RefTypeSyntax refType && refType.Name.Equals("null"))
                return Boolean.True;
            var type = node.TypeSyntax.ResolveType(TypeContext);
            var valueType = value.GetType();
            if (TypeUtils.AreReferenceAssignable(type, valueType))
                return Boolean.True;
            return Boolean.False;
        }

        #region Unary Expression
        public object VisitUnary(UnaryExpression node)
        {
            var value = node.Operand.Accept(this);
            if (node.NodeType == ExpressionType.Parenthesized)
            {
                node.Type = node.Operand.Type;
                return value;
            }
            //modified a++; updated new value
            bool modified = false, updated = true;
            switch (node.NodeType)
            {
                case ExpressionType.PostfixPlusPlus:
                    modified = true;
                    updated = false;
                    break;
                case ExpressionType.PrefixPlusPlus:
                    modified = true;
                    break;
                case ExpressionType.PostfixMinusMinus:
                    modified = true;
                    updated = false;
                    break;
                case ExpressionType.PrefixMinusMinus:
                    modified = true;
                    break;
                case ExpressionType.Bang:
                    // here value is null it is as not defined
                    if (value is null)
                        return Boolean.True;
                    break;
            }
            if (value is null)
                ExecutionException.ThrowNullError(node.Operand, node);
            // no primitive supported it should be wrapped

            //resolve call
            if (node.Method == null)
            {
                Type type = node.Operand.Type;
                ArgumentConversions conversions = new ArgumentConversions(1);
                System.Reflection.MethodInfo method;
                if (type.IsPrimitive)
                {
                    type = TypeProvider.Find(Type.GetTypeCode(type));
                    method = ReflectionUtils.GetOperatorOverload(node.MethodName, conversions, type);
                    conversions.AddFirst(new ParamConversion(0, ReflectionHelpers.ToAny));
                }
                else
                {
                    method = ReflectionUtils.GetOperatorOverload(node.MethodName, conversions, type);
                }
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
                if (node.Operand.NodeType == ExpressionType.Literal)
                    ExecutionException.ThrowNotSupported(node);
                var exp = new AssignmentExpression(node.Operand, new LiteralExpression(obj));
                exp.Accept(this);
            }
            return updated ? obj : value;
        }
        #endregion
    }
}
