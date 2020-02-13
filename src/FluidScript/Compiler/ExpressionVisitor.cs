using FluidScript.Compiler.SyntaxTree;
using FluidScript.Utils;
using System;
using System.Linq;

namespace FluidScript.Compiler
{
    public class ExpressionVisitor : IExpressionVisitor<object>
    {
        private readonly object instance;

        public ExpressionVisitor(object instance)
        {
            this.instance = instance;
        }

        public object Visit(Expression expression)
        {
            return expression.Accept(this);
        }

        object IExpressionVisitor<object>.VisitAnonymousFunction(AnonymousFunctionExpression node)
        {
            throw new NotImplementedException();
        }

        object IExpressionVisitor<object>.VisitAnonymousObject(AnonymousObjectExpression node)
        {
            throw new NotImplementedException();
        }

        object IExpressionVisitor<object>.VisitArrayLiteral(ArrayLiteralExpression node)
        {
            throw new NotImplementedException();
        }

        object IExpressionVisitor<object>.VisitAssignment(AssignmentExpression node)
        {
            var value = node.Right.Accept(this);
            var left = node.Left;
            string name = null;
            var instance = this.instance;
            ExpressionType nodeType = left.NodeType;
            if (nodeType == ExpressionType.Identifier)
            {
                name = left.ToString();
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
                        if (value != null && property.PropertyType != value.GetType())
                        {
                            if (TypeUtils.TryImplicitConvert(value.GetType(), property.PropertyType, out System.Reflection.MethodInfo implictCast))
                            {
                                value = implictCast.Invoke(null, new object[1] { value });
                            }
                        }
                        property.SetValue(instance, value, new object[0]);
                    }
                }
                else if (instance is Dynamic.IRuntimeMetaObjectProvider runtime)
                {
                    var metaObject = runtime.GetMetaObject();
                    metaObject.BindSetMember(name, node.Right.Type, value);
                }
            }
            if (nodeType == ExpressionType.MemberAccess)
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
                    .FindMembers(System.Reflection.MemberTypes.Method, Utils.TypeUtils.Any, HasMember, "set_Item");
                var indexer = Utils.DynamicUtils.BindToMethod(indexers, args, out Reflection.Emit.ParamBindList bindings);
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
                args = new object[2] { left, right };
                var leftType = left.GetType();
                var rightType = right.GetType();
                if (leftType.IsPrimitive && rightType.IsPrimitive)
                {
                    left = FSConvert.ToAny(left);
                    leftType = left.GetType();
                    right = FSConvert.ToAny(right);
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
            args[0] = convert == null ? first : convert.ReflectedType != TypeUtils.BooleanType ? Boolean.True :
                convert.Invoke(null, new object[] { first });
            convert = TypeUtils.GetBooleanOveraload(second.GetType());
            //No bool conversion default true object exist
            args[1] = convert == null ? second : convert.ReflectedType != TypeUtils.BooleanType ? Boolean.True :
                convert.Invoke(null, new object[] { second });
            return nodeType == ExpressionType.AndAnd ? Helpers.LogicalAnd : Helpers.LogicalOr;
        }

        private static System.Reflection.MethodInfo VisitCompare(string opName, ref object[] args)
        {
            object first = args[0];
            object second = args[1];
            // todo correct method binding

            if (first is null || second is null)
                return Helpers.IsEquals;
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



        object IExpressionVisitor<object>.VisitCall(InvocationExpression node)
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
                obj = this.instance;
                var methods = GetInstanceMethod(obj, name);

                if (methods.Length == 0)
                    throw new Exception(string.Concat("method '", name, "' not found in execution of ", node));
                method = DynamicUtils.BindToMethod(methods, args, out bindings);
            }
            else if (nodeType == ExpressionType.MemberAccess)
            {
                var exp = (MemberExpression)target;
                obj = exp.Target.Accept(this);
                name = exp.Name;
                var methods = GetInstanceMethod(obj, name);
                if (methods.Length == 0)
                {
                    if (obj is Dynamic.IRuntimeMetaObjectProvider runtime)
                    {
                        var metaObject = runtime.GetMetaObject();
                        return metaObject.BindInvokeMemeber(name, args);
                    }
                    throw new Exception(string.Concat("method '", name, "' not found in execution of ", node));
                }
                method = DynamicUtils.BindToMethod(methods, args, out bindings);
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
                return m.Name.Equals(name, StringComparison.OrdinalIgnoreCase);
            }
            return resultType
                  .GetMethods(TypeUtils.Any)
                  .Where(HasMethod).ToArray();
        }

        object IExpressionVisitor<object>.VisitDeclaration(VariableDeclarationExpression node)
        {
            throw new NotImplementedException();
        }

        object IExpressionVisitor<object>.VisitIndex(IndexExpression node)
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
                    .FindMembers(System.Reflection.MemberTypes.Method, TypeUtils.Any, HasMember, "get_Item");
                var indexer = DynamicUtils.BindToMethod(indexers, args, out Reflection.Emit.ParamBindList bindings);
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

        bool HasMember(System.Reflection.MemberInfo m, object filter)
        {
            return filter.Equals(m.Name);
        }

        object IExpressionVisitor<object>.VisitMember(MemberExpression node)
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
            else if (target is Dynamic.IRuntimeMetaObjectProvider runtime)
            {
                var metaObject = runtime.GetMetaObject();
                var result = metaObject.BindGetMember(node.Name);
                node.Type = result.Type;
                value = result.Value;
            }
            return value;
        }

        object IExpressionVisitor<object>.VisitMember(NameExpression node)
        {
            string name = node.Name;
            object value = null;
            var instance = this.instance;
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
            return value;
        }

        object IExpressionVisitor<object>.VisitNull(NullExpression node)
        {
            return null;
        }

        object IExpressionVisitor<object>.VisitNullPropegator(NullPropegatorExpression node)
        {
            throw new NotImplementedException();
        }

        object IExpressionVisitor<object>.VisitTernary(TernaryExpression node)
        {
            throw new NotImplementedException();
        }

        object IExpressionVisitor<object>.VisitThis(ThisExpression node)
        {
            return instance;
        }

        object IExpressionVisitor<object>.VisitUnary(UnaryExpression node)
        {
            throw new NotImplementedException();
        }
    }
}
