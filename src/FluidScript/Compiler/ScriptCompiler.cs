using FluidScript.Compiler.SyntaxTree;
using FluidScript.Utils;
using System;

namespace FluidScript.Compiler
{
    /// <summary>
    /// Evaluates an <see cref="Expression"/> using reflection
    /// </summary>
    public class ScriptCompiler : CompilerBase
    {
        private readonly object target;
        private object locals;

        public ScriptCompiler() : this(GlobalObject.Instance)
        {
        }

        public ScriptCompiler(object target)
        {
            this.target = target;
        }

        public override object Target => target;

        public object Invoke(Expression expression)
        {
            return expression.Accept(this);
        }

        public object Invoke(object target, Expression expression)
        {
            locals = target;
            return expression.Accept(this);
        }

        protected override object FindTarget(InvocationExpression node, object[] args)
        {
            //Resolve call again
            return ResolveCall(node, args);
        }

        public override object VisitAssignment(AssignmentExpression node)
        {
            var value = node.Right.Accept(this);
            var left = node.Left;
            string name = null;
            var obj = locals;
            ExpressionType nodeType = left.NodeType;
            Binders.IBinder binder = null;
            Type type = null;
            if (nodeType == ExpressionType.Identifier)
            {
                var exp = (NameExpression)left;
                name = exp.Name;
                if (obj == null)
                    ExecutionException.ThrowNullError(node.Left, node);
                if (TypeUtils.TryFindMember(obj.GetType(), name, out binder) == false && obj is Runtime.IMetaObjectProvider runtime)
                {
                    var result = runtime.GetMetaObject().BindSetMember(name, node.Right.Type, value).Value;
                    value = result.Value;
                    node.Type = result.Type;
                    return value;
                }
                else
                {
                    obj = Target;
                    TypeUtils.TryFindMember(obj.GetType(), name, out binder);
                }
                exp.Binder = binder;
            }
            else if (nodeType == ExpressionType.MemberAccess)
            {
                var exp = (MemberExpression)left;
                obj = exp.Target.Accept(this);
                name = exp.Name;
                if (exp.Binder is null && TypeUtils.TryFindMember(exp.Target.Type, name, out binder))
                    exp.Binder = binder;
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
                    var indexers = exp.Target.Type
                    .GetMember("set_Item", System.Reflection.MemberTypes.Method, TypeUtils.Any);
                    var indexer = TypeHelpers.BindToMethod(indexers, args, out Binders.ArgumentConversions conversions);
                    if (indexer == null)
                        ExecutionException.ThrowMissingIndexer(exp.Target.Type, "set", exp.Target, node);

                    exp.Conversions = conversions;
                    exp.Setter = indexer;
                    // ok to be node.Right.Type instead of indexer.GetParameters().Last().ParameterType
                    Binders.Conversion valueBind = conversions[args.Length - 1];
                    node.Type = (valueBind == null) ? node.Right.Type : valueBind.Type;
                }
                exp.Conversions.Invoke(ref args);
                exp.Setter.Invoke(obj, args);
                return value;
            }
            if (binder == null)
            {
                if (obj is Runtime.IMetaObjectProvider runtime)
                {
                    //todo binder for dynamic
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
            if (!TypeUtils.AreReferenceAssignable(binder.Type, type) && TypeUtils.TryImplicitConvert(type, binder.Type, out System.Reflection.MethodInfo method))
            {
                // implicit casting
                value = method.Invoke(null, new object[] { value });
            }
            else
            {
                ExecutionException.ThrowInvalidCast(binder.Type, node);
            }
            binder.Set(obj, value);
            node.Type = type;
            return value;
        }

        protected override object ResolveCall(InvocationExpression node, object[] args)
        {
            var target = node.Target;
            System.Reflection.MethodInfo method = null;
            ExpressionType nodeType = node.Target.NodeType;
            Binders.ArgumentConversions conversions = null;
            string name;
            object obj;
            if (nodeType == ExpressionType.Identifier)
            {
                var exp = (NameExpression)target;
                name = exp.Name;
                obj = locals;
                method = TypeHelpers.FindMethod(name, obj.GetType(), args, out conversions);
                if (method == null)
                {
                    // find in target
                    obj = Target;
                    method = TypeHelpers.FindMethod(name, obj.GetType(), args, out conversions);
                    // if not methods
                    if (method == null)
                        ExecutionException.ThrowMissingMethod(obj.GetType(), name, node);
                }
            }
            else if (nodeType == ExpressionType.MemberAccess)
            {
                var exp = (MemberExpression)target;
                obj = exp.Target.Accept(this);
                name = exp.Name;
                var methods = TypeHelpers.FindMethod(name, exp.Target.Type, args, out conversions);
                if (method == null)
                {
                    if (obj is Runtime.IMetaObjectProvider runtime)
                    {
                        exp.Binder = runtime.GetMetaObject().BindGetMember(name);
                        var value = exp.Binder.Get(obj);
                        if (value is Delegate del)
                        {
                            conversions = new Binders.ArgumentConversions(args.Length);
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
                conversions = new Binders.ArgumentConversions(args.Length);
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

        public override object VisitDeclaration(VariableDeclarationExpression node)
        {
            throw new NotSupportedException();
        }

        /// Member Visit
        public override object VisitMember(MemberExpression node)
        {
            var target = node.Target.Accept(this);
            if (TypeUtils.TryFindMember(node.Target.Type, node.Name, out Binders.IBinder binder) == false)
            {
                if (target is Runtime.IMetaObjectProvider runtime)
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
            return node.Binder.Get(target);
        }

        public override object VisitMember(NameExpression node)
        {
            string name = node.Name;
            object obj = locals;
            if (TypeUtils.TryFindMember(obj.GetType(), name, out Binders.IBinder binder) == false && obj is Runtime.IMetaObjectProvider)
            {
                Runtime.IMetaObjectProvider dynamic = (Runtime.IMetaObjectProvider)obj;
                binder = dynamic.GetMetaObject().BindGetMember(name);
                if (binder != null)
                    goto done;
                obj = Target;
                //find in the class level
                if (TypeUtils.TryFindMember(obj.GetType(), name, out binder) == false && obj is Runtime.IMetaObjectProvider)
                {
                    dynamic = (Runtime.IMetaObjectProvider)obj;
                    binder = dynamic.GetMetaObject().BindGetMember(name);
                }
                if (binder is null)
                {
                    ExecutionException.ThrowMissingMember(obj.GetType(), name, node);
                }
            }
        done:
            node.Binder = binder;
            node.Type = binder.Type;
            return binder.Get(obj);
        }
    }
}
