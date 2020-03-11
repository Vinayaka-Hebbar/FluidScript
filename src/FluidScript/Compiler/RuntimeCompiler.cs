using FluidScript.Compiler.SyntaxTree;
using FluidScript.Utils;
using System;
using System.Linq;

namespace FluidScript.Compiler
{
    /// <summary>
    /// expression visitor for instance does not support statements
    /// </summary>
    public class RuntimeCompiler : IExpressionVisitor<object>
    {
        public object Locals { get; set; }

        public object Target { get; }

        private static readonly object Empty = new object();

        public RuntimeCompiler(object target)
        {
            Target = target;
            Locals = Empty;
        }

        public RuntimeCompiler(object target, object locals)
        {
            Target = target;
            Locals = locals;
        }


        public object Invoke(Expression expression)
        {
            return expression.Accept(this);
        }

        /// <summary>
        /// invoke the expression
        /// </summary>
        object IExpressionVisitor<object>.Visit(Expression expression)
        {
            return expression.Accept(this);
        }

        object IExpressionVisitor<object>.VisitAnonymousFunction(AnonymousFunctionExpression node)
        {
            return node.Compile(Target.GetType(), this);
        }

        object IExpressionVisitor<object>.VisitAnonymousObject(AnonymousObjectExpression node)
        {
            var members = node.Members;
            var obj = new Runtime.DynamicObject(members.Length);
            for (int index = 0; index < members.Length; index++)
            {
                AnonymousObjectMember item = members[index];
                var value = item.Expression.Accept(this);
                obj.Add(item.Name, value);
            }
            return obj;
        }

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

        object IExpressionVisitor<object>.VisitAssignment(AssignmentExpression node)
        {
            var value = node.Right.Accept(this);
            var left = node.Left;
            string name = null;
            var obj = Locals;
            ExpressionType nodeType = left.NodeType;
            Binders.IBinder binder = null;
            System.Type type = null;
            if (nodeType == ExpressionType.Identifier)
            {
                name = left.ToString();
                if (obj == null)
                    ExecutionException.ThrowNullError(node.Left, node);
                binder = TypeUtils.GetMember(obj.GetType(), name);
                if (binder == null)
                {
                    if (obj is Runtime.IRuntimeMetaObjectProvider runtime)
                    {
                        var result = runtime.GetMetaObject().BindSetMember(name, node.Right.Type, value).Value;
                        value = result.Value;
                        node.Type = result.Type;
                        return value;
                    }
                    obj = Target;
                    binder = TypeUtils.GetMember(obj.GetType(), name);
                }
            }
            else if (nodeType == ExpressionType.MemberAccess)
            {
                var exp = (MemberExpression)left;
                obj = exp.Target.Accept(this);
                name = exp.Name;
                binder = TypeUtils.GetMember(exp.Target.Type, name);
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
                var indexer = TypeHelpers.BindToMethod(indexers, args, out Binders.ArgumenConversions conversions);
                if (indexer == null)
                    ExecutionException.ThrowMissingIndexer(exp.Target.Type, "set", node);
                exp.Setter = indexer;
                foreach (var conversion in conversions)
                {
                    if (conversion.ConversionType == Binders.ConversionType.Convert)
                    {
                        args[conversion.Index] = conversion.Invoke(args);
                    }
                    // No params
                }
                type = indexer.ReturnType;
                node.Type = type;
                exp.Setter.Invoke(obj, args.ToArray());
                return value;
            }
            else if (binder is null && obj is Runtime.IRuntimeMetaObjectProvider runtime)
            {
                var result = runtime.GetMetaObject().BindSetMember(name, node.Right.Type, value).Value;
                value = result.Value;
                type = result.Type;
            }
            else
            {
                ExecutionException.ThrowMissingMember(obj.GetType(), name, node.Left, node);
            }
            node.Type = type;
            binder.Set(obj, value);
            return value;
        }

        object IExpressionVisitor<object>.VisitBinary(BinaryExpression node)
        {
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
                case ExpressionType.StarStar:
                    method = ReflectionHelpers.MathPow;
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
                // both are primitive it shouble be wrapped
                if (leftType.IsPrimitive && rightType.IsPrimitive)
                {
                    args[0] = FSConvert.ToAny(left);
                    leftType = left.GetType();
                    args[1] = FSConvert.ToAny(right);
                    rightType = right.GetType();
                }
                method = TypeUtils.
                   GetOperatorOverload(opName, out Binders.ArgumenConversions conversions, leftType, rightType);
                // null method handled
                foreach (var conversion in conversions)
                {
                    if (conversion.ConversionType == Binders.ConversionType.Convert)
                    {
                        args[conversion.Index] = conversion.Invoke(args);
                    }
                    // No Params
                }
            }
            if (method == null)
                ExecutionException.ThrowInvalidOp(node);
            node.Method = method;
            node.Type = method.ReturnType;
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
                GetOperatorOverload(opName, out Binders.ArgumenConversions conversions, leftType, rightType);
            // null method handled
            foreach (var binder in conversions)
            {
                if (binder.ConversionType == Binders.ConversionType.Convert)
                {
                    args[binder.Index] = binder.Invoke(args);
                }
                // No Params
            }
            return method;
        }

        object IExpressionVisitor<object>.VisitCall(InvocationExpression node)
        {
            var target = node.Target;
            object obj = null;
            object[] args = node.Arguments.Map(arg => arg.Accept(this));
            string name = null;
            System.Reflection.MethodInfo method = null;
            ExpressionType nodeType = node.Target.NodeType;
            Binders.ArgumenConversions conversions = null;
            if (nodeType == ExpressionType.Identifier)
            {
                name = target.ToString();
                obj = Locals;
                var methods = TypeHelpers.GetPublicMethods(obj, name);
                if (methods.Length > 0)
                    method = TypeHelpers.BindToMethod(methods, args, out conversions);
                if (method == null)
                {
                    // find in target
                    obj = Target;
                    methods = TypeHelpers.GetPublicMethods(obj, name);
                    // if not methods
                    if (methods.Length == 0)
                        ExecutionException.ThrowMissingMethod(obj.GetType(), name, node);
                    method = TypeHelpers.BindToMethod(methods, args, out conversions);
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
                else if (obj is Runtime.IRuntimeMetaObjectProvider runtime)
                {
                    var del = runtime.GetMetaObject().GetDelegate(name, args, out conversions);
                    obj = del.Target;
                    method = del.Method;
                }
                else
                {
                    ExecutionException.ThrowMissingMethod(exp.Target.Type, name, node);
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
                System.Reflection.MethodInfo invoke = res.GetType().GetMethod(name);
                conversions = new Binders.ArgumenConversions();
                if (!TypeHelpers.MatchesTypes(invoke, args, conversions))
                    ExecutionException.ThrowArgumentMisMatch(node.Target, node);
                method = invoke;
                obj = res;
            }
            foreach (var conversion in conversions)
            {
                if (conversion.ConversionType == Binders.ConversionType.Convert)
                {
                    args[conversion.Index] = conversion.Invoke(args);
                }
                else if (conversion.ConversionType == Binders.ConversionType.ParamArray)
                {
                    args = (object[])conversion.Invoke(args);
                    break;
                }
            }
            if (method == null)
                ExecutionException.ThrowMissingMethod(obj.GetType(), name, node);
            node.Method = method;
            node.Type = method.ReturnType;
            return method.Invoke(obj, args);
        }

        object IExpressionVisitor<object>.VisitDeclaration(VariableDeclarationExpression node)
        {
            throw new NotImplementedException();
        }

        object IExpressionVisitor<object>.VisitIndex(IndexExpression node)
        {
            var obj = node.Target.Accept(this);
            if (obj is null)
                ExecutionException.ThrowNullError(node.Target, node);
            var type = obj.GetType();
            object value = null;
            var args = node.Arguments.Map(arg => arg.Accept(this));
            if (type.IsArray)
            {
                var list = (System.Collections.IList)obj;
                type = type.GetElementType();
                var first = args.Select(arg => Convert.ToInt32(arg)).FirstOrDefault();
                value = list[first];
            }
            else
            {
                var indexers = type
                    .GetMember("get_Item", System.Reflection.MemberTypes.Method, TypeUtils.Any);
                var indexer = TypeHelpers.BindToMethod(indexers, args, out Binders.ArgumenConversions conversions);
                if (indexer == null)
                    ExecutionException.ThrowMissingIndexer(node.Target.Type, "get", node);
                node.Getter = indexer;
                foreach (var conversion in conversions)
                {
                    if (conversion.ConversionType == Binders.ConversionType.Convert)
                    {
                        args[conversion.Index] = conversion.Invoke(args);
                    }
                    // No params array
                }
                type = indexer.ReturnType;
                value = node.Getter.Invoke(obj, args);
            }
            node.Type = type;
            return value;
        }

        object IExpressionVisitor<object>.VisitLiteral(LiteralExpression node)
        {
            return node.ReflectedValue;
        }

        object IExpressionVisitor<object>.VisitMember(MemberExpression node)
        {
            var target = node.Target.Accept(this);
            var binder = TypeUtils.GetMember(node.Target.Type, node.Name);
            if (binder is null)
            {
                if (target is Runtime.IRuntimeMetaObjectProvider runtime)
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
            return binder.Get(target);
        }

        object IExpressionVisitor<object>.VisitMember(NameExpression node)
        {
            string name = node.Name;
            object obj = Locals;
            Binders.IBinder binder = TypeUtils.GetMember(obj.GetType(), name);
            if (binder is null)
            {
                obj = Target;
                //find in the class level
                binder = TypeUtils.GetMember(obj.GetType(), name);
                if (binder is null && obj is Runtime.IRuntimeMetaObjectProvider dynamic)
                {
                    binder = dynamic.GetMetaObject().BindGetMember(name);
                }
                else
                {
                    ExecutionException.ThrowMissingMember(obj.GetType(), name, node);
                }
            }
            node.Binder = binder;
            node.Type = binder.Type;
            return binder.Get(obj);
        }

        object IExpressionVisitor<object>.VisitNull(NullExpression node)
        {
            return null;
        }

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

        object IExpressionVisitor<object>.VisitThis(ThisExpression node)
        {
            return Target;
        }

        object IExpressionVisitor<object>.VisitSizeOf(SizeOfExpression node)
        {
            var value = node.Value.Accept(this);
            if (value == null)
                return 0;
            if (value is System.Collections.ICollection collection)
                return collection.Count;
            return System.Runtime.InteropServices.Marshal.SizeOf(value);
        }

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
            // todo if its of to be null or not
            if (value is null)
                ExecutionException.ThrowNullError(node);
            Type type = value.GetType();
            // not primitive supported it should be wrapped
            if (type.IsPrimitive)
            {
                value = FSConvert.ToAny(value);
                type = value.GetType();
            }
            //todo conversion
            var method = TypeUtils.GetOperatorOverload(name, out Binders.ArgumenConversions conversions, type);
            if (method == null)
                ExecutionException.ThrowInvalidOp(node);
            var args = new object[1] { value };
            foreach (var conversion in conversions)
            {
                if (conversion.ConversionType == Binders.ConversionType.Convert)
                    value = conversion.Invoke(args);
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
    }
}
