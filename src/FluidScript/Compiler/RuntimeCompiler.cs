using FluidScript.Compiler.SyntaxTree;
using FluidScript.Utils;
using System;

namespace FluidScript.Compiler
{
    /// <summary>
    /// expression visitor for instance does not support statements
    /// </summary>
    public sealed class RuntimeCompiler : CompilerBase
    {
        private static readonly object Empty = new object();

        public RuntimeCompiler() : base(GlobalObject.Instance)
        {
            Locals = Empty;
        }

        public RuntimeCompiler(object target) : base(target)
        {
            Locals = Empty;
        }

        public RuntimeCompiler(object target, object locals) : base(target)
        {
            Locals = locals;
        }

        object locals;
        public object Locals
        {
            get => locals;
            set
            {
                locals = value ?? Empty;
            }
        }

        static System.Reflection.FieldInfo m_localField;

        internal static System.Reflection.FieldInfo LocalField
        {
            get
            {
                if (m_localField == null)
                    m_localField = typeof(RuntimeCompiler).GetField(nameof(locals), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                return m_localField;
            }
        }

        public object Invoke(Expression expression)
        {
            return expression.Accept(this);
        }

        public override object VisitAssignment(AssignmentExpression node)
        {
            var value = node.Right.Accept(this);
            var left = node.Left;
            string name = null;
            var obj = Locals;
            ExpressionType nodeType = left.NodeType;
            Binders.IBinder binder = null;
            Type type = null;
            if (nodeType == ExpressionType.Identifier)
            {
                var exp = (NameExpression)left;
                binder = exp.Binder;
                if (binder == null)
                {
                    name = exp.Name;
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
                    exp.Binder = binder;
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
                    var indexers = exp.Target.Type
                    .GetMember("set_Item", System.Reflection.MemberTypes.Method, TypeUtils.Any);
                    var indexer = TypeHelpers.BindToMethod(indexers, args, out Binders.ArgumentConversions conversions);
                    if (indexer == null)
                        ExecutionException.ThrowMissingIndexer(exp.Target.Type, "set", exp.Target, node);

                    exp.Conversions = conversions;
                    exp.Setter = indexer;
                    // ok to be node.Right.Type instead of indexer.GetParameters().Last().ParameterType
                    Binders.Conversion valueBind = conversions.At(args.Length - 1);
                    node.Type = (valueBind == null) ? node.Right.Type : valueBind.Type;
                }
                args = exp.Conversions.Invoke(args);
                exp.Setter.Invoke(obj, args);
                return value;
            }
            if (binder == null)
            {
                if (obj is Runtime.IRuntimeMetaObjectProvider runtime)
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
                    exp.Binder = new Binders.FieldBinder(TargetField);
                }
                else
                {
                    exp.Binder = new Binders.FieldBinder(LocalField);
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
                    exp.Binder = runtime.GetMetaObject().BindGetMember(name);
                    var value = exp.Binder.Get(obj);
                    if (value is Delegate del)
                    {
                        conversions = new Binders.ArgumentConversions();
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
            else
            {
                var res = target.Accept(this);
                if (res == null)
                    ExecutionException.ThrowNullError(target, node);
                if (!(res is Delegate))
                    ExecutionException.ThrowInvalidOp(target, node);
                name = "Invoke";
                System.Reflection.MethodInfo invoke = res.GetType().GetMethod(name);
                conversions = new Binders.ArgumentConversions();
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
            return null;
        }

        public override object VisitDeclaration(VariableDeclarationExpression node)
        {
            throw new NotSupportedException();
        }

        public override object VisitMember(MemberExpression node)
        {
            var target = node.Target.Accept(this);
            if (node.Binder == null)
            {
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
            }
            return node.Binder.Get(target);
        }

        public override object VisitMember(NameExpression node)
        {
            string name = node.Name;
            object obj = Locals;
            Binders.IBinder binder = TypeUtils.GetMember(obj.GetType(), name);
            if (binder is null)
            {
                obj = Target;
                //find in the class level
                binder = TypeUtils.GetMember(obj.GetType(), name);
                if (binder is null)
                {
                    if (obj is Runtime.IRuntimeMetaObjectProvider dynamic)
                    {
                        binder = dynamic.GetMetaObject().BindGetMember(name);
                    }
                    else
                    {
                        ExecutionException.ThrowMissingMember(obj.GetType(), name, node);
                    }
                }
            }
            node.Binder = binder;
            node.Type = binder.Type;
            return binder.Get(obj);
        }
    }
}
