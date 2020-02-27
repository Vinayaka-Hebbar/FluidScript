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
            return node.Compile(this);
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
            System.Reflection.MemberInfo member = null;
            System.Type type = null;
            if (nodeType == ExpressionType.Identifier)
            {
                name = left.ToString();
                if (obj == null)
                    ExecutionException.ThrowNullError(node.Left, node);
                member = obj.GetType().GetMember(name).FirstOrDefault();
                if (member == null)
                {
                    if (obj is Runtime.IRuntimeMetaObjectProvider runtime)
                    {
                        var result = runtime.GetMetaObject().BindSetMember(name, node.Right.Type, value).Value;
                        value = result.Value;
                        node.Type = result.Type;
                        return value;
                    }
                    obj = Target;
                    member = TypeHelpers.GetMember(obj, name);
                }
            }
            else if (nodeType == ExpressionType.MemberAccess)
            {
                var exp = (MemberExpression)left;
                obj = exp.Target.Accept(this);
                name = exp.Name;
                member = TypeHelpers.GetMember(obj, name);
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
                var indexer = TypeHelpers.BindToMethod(indexers, args, out Binders.ArgumentBinderList bindings);
                if (indexer == null)
                    ExecutionException.ThrowMissingIndexer(obj, "set", node);
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
                exp.Setter.Invoke(obj, args.ToArray());
                return value;
            }
            if (member != null)
            {
                value = TypeHelpers.InvokeSet(member, obj, value, out type);
            }
            else if (obj is Runtime.IRuntimeMetaObjectProvider runtime)
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
                obj = Locals;
                var methods = TypeHelpers.GetPublicMethods(obj, name);
                if (methods.Length > 0)
                    method = TypeHelpers.BindToMethod(methods, args, out bindings);
                if (method == null)
                {
                    // find in target
                    obj = Target;
                    methods = TypeHelpers.GetPublicMethods(obj, name);
                    // if not methods
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
                else if (obj is Runtime.IRuntimeMetaObjectProvider runtime)
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

        object IExpressionVisitor<object>.VisitMember(MemberExpression node)
        {
            var target = node.Target.Accept(this);
            object value = null;
            var member = TypeHelpers.GetMember(target, node.Name);
            Type type = null;
            // member not found is ok
            if (member != null)
            {
                value = TypeHelpers.InvokeGet(member, target, out type);
            }
            else if (target is Runtime.IRuntimeMetaObjectProvider runtime)
            {
                var metaObject = runtime.GetMetaObject();
                var result = metaObject.BindGetMember(node.Name);
                if (result.HasValue)
                {
                    var data = result.Value;
                    type = data.Type;
                    value = data.Value;
                }
            }
            else
            {
                ExecutionException.ThrowMissingMember(target, node.Name, node);
            }
            node.Type = type;
            return value;
        }

        object IExpressionVisitor<object>.VisitMember(NameExpression node)
        {
            string name = node.Name;
            var obj = Locals;
            //find in the class level
            var m = TypeHelpers.GetMember(obj, name);
            object value;
            if (m == null)
            {
                if (obj is Runtime.IRuntimeMetaObjectProvider dynamic)
                {
                    var result = dynamic.GetMetaObject().BindGetMember(name);
                    if (result.HasValue)
                    {
                        var data = result.Value;
                        node.Type = data.Type;
                        value = data.Value;
                        return value;
                    }
                }
                obj = Target;
                m = TypeHelpers.GetMember(obj, name);
            }
            if (m == null)
                ExecutionException.ThrowMissingMember(obj, name, node);
            value = TypeHelpers.InvokeGet(m, obj, out Type type);
            node.Type = type;
            return value;
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
    }
}
