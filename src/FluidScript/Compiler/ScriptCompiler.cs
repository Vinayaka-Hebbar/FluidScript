﻿using FluidScript.Compiler.SyntaxTree;
using FluidScript.Extensions;
using FluidScript.Runtime;
using FluidScript.Utils;
using System;

namespace FluidScript.Compiler
{
    /// <summary>
    /// Evaluates an <see cref="Expression"/> using reflection
    /// </summary>
    public class ScriptCompiler : CompilerBase, ICompileProvider
    {
        readonly object target;
        object locals;

        public ScriptCompiler() : this(GlobalObject.Instance)
        {
        }

        public ScriptCompiler(object target)
        {
            this.target = target;
        }

        static ScriptCompiler _defaultInstance;
        public static ScriptCompiler Instance
        {
            get
            {
                if (_defaultInstance == null)
                    System.Threading.Interlocked.CompareExchange(ref _defaultInstance, new ScriptCompiler(GlobalObject.Instance), null);
                return _defaultInstance;
            }
        }

        public override object Target => target;

        Type targetType;
        public Type TargetType
        {
            get
            {
                if (targetType == null)
                    targetType = target.GetType();
                return targetType;
            }
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.Synchronized)]
        public object Invoke(Expression expression, object target = null)
        {
            locals = target;
            return expression.Accept(this);
        }

        protected override object FindTarget(InvocationExpression node, object[] args)
        {
            // Resolve call again
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
            if (nodeType == ExpressionType.Identifier)
            {
                var exp = (NameExpression)left;
                name = exp.Name;
                if (obj == null || TypeExtensions.TryFindMember(obj.GetType(), name, ReflectionUtils.AnyPublic, out binder) == false)
                {
                    if (obj is IMetaObjectProvider runtime)
                    {
                        var result = runtime.GetMetaObject().BindSetMember(name, node.Right.Type, value).Value;
                        value = result.Value;
                        node.Type = result.Type;
                        return value;
                    }
                    else if (TargetType.TryFindMember(name, ReflectionUtils.AnyPublic, out binder))
                    {
                        obj = Target;
                    }
                }
                exp.Binder = binder;
            }
            else if (nodeType == ExpressionType.MemberAccess)
            {
                var exp = (MemberExpression)left;
                obj = exp.Target.Accept(this);
                name = exp.Name;
                if (exp.Binder is null && exp.Target.Type.TryFindMember(name, ReflectionUtils.AnyPublic, out binder))
                    exp.Binder = binder;
                else if (obj is IMetaObjectProvider runtime)
                {
                    //todo binder for dynamic
                    var result = runtime.GetMetaObject().BindSetMember(name, node.Right.Type, value).Value;
                    value = result.Value;
                    node.Type = result.Type;
                    return value;
                }
            }
            else if (nodeType == ExpressionType.Indexer)
            {
                return AssignIndexer(node, value);
            }
            if (binder == null)
            {
                ExecutionException.ThrowMissingMember(obj.GetType(), name, node.Left, node);
            }
            Type type = node.Right.Type;
            if (!TypeUtils.AreReferenceAssignable(binder.Type, type))
            {
                if (TypeUtils.TryImplicitConvert(type, binder.Type, out System.Reflection.MethodInfo method))
                    // implicit casting
                    value = method.Invoke(null, new object[] { value });
                else
                {
                    ExecutionException.ThrowInvalidCast(binder.Type, node);
                }
            }
            binder.Set(obj, value);
            node.Type = binder.Type;
            return value;
        }

        protected override object ResolveCall(InvocationExpression node, object[] args)
        {
            var target = node.Target;
            System.Reflection.MethodInfo method = null;
            ExpressionType nodeType = node.Target.NodeType;
            ArgumentConversions conversions = null;
            string name;
            object obj;
            if (nodeType == ExpressionType.Identifier)
            {
                var exp = (NameExpression)target;
                name = exp.Name;
                obj = locals;
                if (obj == null || TypeHelpers.TryFindMethod(name, obj.GetType(), args, out method, out conversions) == false)
                {
                    // find in target
                    obj = Target;
                    // if not methods
                    if (TypeHelpers.TryFindMethod(name, obj.GetType(), args, out method, out conversions) == false)
                        ExecutionException.ThrowMissingMethod(obj.GetType(), name, node);
                }
            }
            else if (nodeType == ExpressionType.MemberAccess)
            {
                var exp = (MemberExpression)target;
                obj = exp.Target.Accept(this);
                name = exp.Name;
                if (TypeHelpers.TryFindMethod(name, exp.Target.Type, args, out method, out conversions) == false)
                {
                    if (obj is IMetaObjectProvider runtime)
                    {
                        exp.Binder = runtime.GetMetaObject().BindGetMember(name);
                        var value = exp.Binder.Get(obj);
                        if (value is Delegate del)
                        {
                            conversions = new ArgumentConversions(args.Length);
                            name = nameof(Action.Invoke);
                            method = del.GetType().GetMethod(name);
                            if (TypeUtils.MatchesTypes(method, args, conversions))
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
                conversions = new ArgumentConversions(args.Length);
                if (!TypeUtils.MatchesTypes(invoke, args, conversions))
                    ExecutionException.ThrowArgumentMisMatch(node.Target, node);
                method = invoke;
                obj = res;
            }
            if (method == null)
                ExecutionException.ThrowMissingMethod(obj.GetType(), name, node);
            node.Method = method;
            node.Type = method.ReturnType;
            node.Conversions = conversions;
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
            if (node.Target.Type.TryFindMember( node.Name, ReflectionUtils.AnyPublic, out Binders.IBinder binder) == false)
            {
                if (target is IMetaObjectProvider runtime)
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
            if (obj == null || TypeExtensions.TryFindMember(obj.GetType(), name, ReflectionUtils.AnyPublic, out Binders.IBinder binder) == false)
            {
                IMetaObjectProvider dynamic;
                if (obj is IMetaObjectProvider)
                {
                    dynamic = (IMetaObjectProvider)obj;
                    binder = dynamic.GetMetaObject().BindGetMember(name);
                }
                else if (TargetType.TryFindMember(name, ReflectionUtils.AnyPublic, out binder) == false)
                {
                    // find in the class level
                    obj = Target;
                    if (obj is IMetaObjectProvider)
                    {
                        dynamic = (IMetaObjectProvider)obj;
                        binder = dynamic.GetMetaObject().BindGetMember(name);
                    }
                    else if (typeof(GlobalObject).TryFindMember(name, ReflectionUtils.AnyPublic, out binder))
                    {
                        obj = GlobalObject.Instance;
                    }
                }
            }
            node.Binder = binder ?? throw ExecutionException.ThrowMissingMember(obj.GetType(), name, node);
            node.Type = binder.Type;
            return binder.Get(obj);
        }
    }
}
