﻿using FluidScript.Compiler.SyntaxTree;
using FluidScript.Extensions;
using FluidScript.Runtime;
using FluidScript.Utils;
using System;

namespace FluidScript.Compiler
{
    /// <summary>
    /// Runtime evaluation of Syntax tree with <see cref="Locals"/>
    /// <list type="bullet">it will be not same as compiled </list>
    /// </summary>
    public class RuntimeCompiler : CompilerBase, IStatementVisitor, IRuntimeCompiler
    {
        private readonly BranchContext context = new BranchContext();
        private readonly RuntimeVariables locals;

        private readonly object m_global;
        private object m_target;

        /// <summary>
        /// New runtime evaluation with <see cref="GlobalObject"/> target
        /// </summary>
        public RuntimeCompiler() : this(GlobalObject.Instance)
        {
        }

        /// <summary>
        /// New runtime evaluation 
        /// </summary>
        /// <param name="target">Global target for execution</param>
        public RuntimeCompiler(object target)
        {
            m_global = target;
            locals = new RuntimeVariables();
        }

        /// <summary>
        /// New runtime evaluation with local values
        /// </summary>
        public RuntimeCompiler(object target, RuntimeVariables locals)
        {
            m_global = target;
            this.locals = locals;
        }

        public ILocalVariables Locals
        {
            get
            {
                return locals;
            }
        }

        private IMemberResolver resolver;
        /// <summary>
        /// Member resolver if fails to find
        /// </summary>
        public IMemberResolver Resolver
        {
            get
            {
                return resolver ?? DefaultResolver.Instance;
            }
            set
            {
                resolver = value;
            }
        }

        static System.Reflection.FieldInfo gField;
        private static System.Reflection.FieldInfo GlobalTargetField
        {
            get
            {
                if (gField is null)
                    gField = typeof(RuntimeCompiler).GetField(nameof(m_global), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                return gField;
            }
        }

        static System.Reflection.FieldInfo cField;
        private static System.Reflection.FieldInfo ClassTargetField
        {
            get
            {
                if (cField is null)
                    cField = typeof(RuntimeCompiler).GetField(nameof(m_target), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                return cField;
            }
        }

        Type globalType;
        public Type GlobalType
        {
            get
            {
                if (globalType is null)
                    globalType = m_global.GetType();
                return globalType;
            }
        }

        public override object Target
        {
            get
            {
                return m_target;
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
            return Invoke(statement, NoTarget);
        }

        /// <summary>
        /// Evaluate the <paramref name="statement"/>
        /// </summary>
        /// <exception cref="IndexOutOfRangeException"/>
        /// <exception cref="NullReferenceException"/>
        /// <exception cref="MissingMethodException"/>
        public object Invoke(Statement statement, object target)
        {
            m_target = target;
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
            return Invoke(expression, NoTarget);
        }

        /// <summary>
        /// Evaluate the <paramref name="expression"/>
        /// </summary>
        /// <exception cref="IndexOutOfRangeException"/>
        /// <exception cref="NullReferenceException"/>
        /// <exception cref="MissingMethodException"/>
        public object Invoke(Expression expression, object target)
        {
            m_target = target;
            return expression.Accept(this);
        }

        /// <summary>
        /// create's a scope for variables inside a block
        /// </summary>
        public IDisposable EnterScope()
        {
            return new RuntimeVariables.RuntimeScope(locals);
        }

        #region Visitors
        /// <inheritdoc/>
        public override object VisitAssignment(AssignmentExpression node)
        {
            var value = node.Right.Accept(this);
            var left = node.Left;
            string name = null;
            object obj = null;
            ExpressionType nodeType = left.NodeType;
            Type type = node.Right.Type;
            Binders.IBinder binder = null;
            if (nodeType == ExpressionType.Identifier)
            {
                var exp = (NameExpression)left;
                name = exp.Name;
                if (locals.TryFindVariable(name, out LocalVariable variable))
                {
                    locals[variable.Index] = value;
                    node.Type = variable.Type;
                    return value;
                }
                binder = exp.Binder;
                if (binder is null)
                {
                    if (m_target != null && TypeExtensions.TryFindMember(m_target.GetType(), name, ReflectionUtils.AnyPublic, out binder))
                    {
                        obj = m_target;
                    }
                    else if (GlobalType.TryFindMember(name, ReflectionUtils.AnyPublic, out binder))
                    {
                        obj = m_global;
                    }
                    else
                    {
                        // not found, add to global
                        locals.InsertAtRoot(name, node.Right.Type, value);
                        node.Type = type;
                        return value;
                    }
                    exp.Binder = binder;
                }
                else if (binder is Binders.ITargetBinder targetBinder)
                {
                    Type declaringType = targetBinder.DeclaringType;
                    if (declaringType == m_target?.GetType())
                    {
                        obj = m_target;
                    }
                    else if (declaringType == GlobalType)
                    {
                        obj = m_global;
                    }
                    else
                    {
                        ExecutionException.ThrowMissingMember(binder.Type, name, exp);
                    }
                }
            }
            else if (nodeType == ExpressionType.MemberAccess)
            {
                var exp = (MemberExpression)left;
                obj = exp.Target.Accept(this);
                name = exp.Name;
                if (binder is null && exp.Target.Type.TryFindMember(name, ReflectionUtils.AnyPublic, out binder))
                {
                    exp.Binder = binder;
                }
                else if (obj is IRuntimeMetadata runtime && runtime.GetOrCreateBinder(name, value, type, out IMemberBinder member))
                {
                    exp.Binder = new Binders.RuntimeMemberBinder(member);
                }
                binder = exp.Binder;

            }
            else if (nodeType == ExpressionType.Indexer)
            {
                return AssignIndexer(node, value);
            }
            if (binder == null)
            {
                // obj type will be only for member expression so no problem
                ExecutionException.ThrowMissingMember(obj.GetType(), name, node.Left, node);
            }
            if (!TypeUtils.AreReferenceAssignable(binder.Type, type))
            {
                if (type.TryImplicitConvert(binder.Type, out System.Reflection.MethodInfo method))
                    // implicit casting
                    value = method.Invoke(null, new object[] { value });
                else
                    ExecutionException.ThrowInvalidCast(binder.Type, node);
            }
            binder.Set(obj, value);
            node.Type = binder.Type;
            return value;
        }

        #region Call

        /// <summary>
        /// Get method
        /// </summary>
        /// <returns>target to invoke</returns>
        protected override object ResolveCall(InvocationExpression node, object[] args)
        {
            var target = node.Target;
            string name;
            ExpressionType nodeType = node.Target.NodeType;
            // invocation target
            object obj;
            ArgumentConversions conversions;
            System.Reflection.MethodInfo method;
            // named expression
            if (nodeType == ExpressionType.Identifier)
            {
                var exp = (NameExpression)target;
                name = exp.Name;
                if (locals.TryFindVariable(name, out LocalVariable variable))
                {
                    var refer = locals[variable.Index] as Delegate;
                    if (refer is null)
                        ExecutionException.ThrowInvalidOp(node);
                    ReflectionUtils.TryGetDelegateMethod(refer, args, out method, out conversions);
                    // only static method can allowed
                    obj = refer;
                    exp.Binder = new Binders.RuntimeVariableBinder(variable, locals);
                }
                else
                {
                    obj = Target;
                    if (m_target != null && m_target.GetType().TryFindMethod(name, args, out method, out conversions))
                        exp.Binder = new Binders.FieldBinder(ClassTargetField);
                    else if (GlobalType.TryFindMethod(name, args, out method, out conversions))
                        exp.Binder = new Binders.FieldBinder(GlobalTargetField);
                    else
                        ExecutionException.ThrowMissingMethod(GlobalType, name, node);
                    exp.Type = method.ReturnType;
                }
            }
            else if (nodeType == ExpressionType.MemberAccess)
            {
                var exp = (MemberExpression)target;
                obj = exp.Target.Accept(this);
                name = exp.Name;
                exp.Type = exp.Target.Type;
                if (exp.Type.TryFindMethod(name, args, out method, out conversions) == false)
                {
                    if (obj is IRuntimeMetadata runtime && runtime.TryGetBinder(name, out IMemberBinder member))
                    {
                        if (member.Get(obj) is Delegate del
                            && ReflectionUtils.TryGetDelegateMethod(del, args, out method, out conversions))
                        {
                            obj = del;
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
                // lamda parameterized invoke
                obj = target.Accept(this);
                if (obj == null)
                    ExecutionException.ThrowNullError(target, node);
                if (!(obj is Delegate))
                    ExecutionException.ThrowInvalidOp(target, node);
                name = ReflectionUtils.InvokeMethod;
                //Multi Delegate Invoke()
                if (!ReflectionUtils.TryGetDelegateMethod(obj, args, out method, out conversions))
                    ExecutionException.ThrowArgumentMisMatch(node.Target, node);
            }

            node.Method = method ?? Resolver.Resolve(node, name, obj, args);
            node.Type = method.ReturnType;
            node.Conversions = conversions;
            return obj;
        }
        #endregion

        /// <inheritdoc/>
        public override object VisitMember(MemberExpression node)
        {
            var target = node.Target.Accept(this);
            if (target is null)
                // target null member cannot be invoked
                ExecutionException.ThrowNullError(node.Target, node);
            if (node.Binder == null)
            {
                if (node.Target.Type.TryFindMember(node.Name, ReflectionUtils.AnyPublic, out Binders.IBinder binder) == false)
                {
                    if (target is IRuntimeMetadata runtime && runtime.TryGetBinder(node.Name, out IMemberBinder member))
                    {
                        binder = new Binders.RuntimeMemberBinder(member);
                    }
                    else
                    {
                        member = Resolver.Resolve(node) ?? throw ExecutionException.ThrowMissingMember(node.Target.Type, node.Name, node);
                        node.Type = member.Type;
                        return null;
                    }
                }
                node.Binder = binder;
                node.Type = binder.Type;
            }
            return node.Binder.Get(target);
        }

        /// <inheritdoc/>
        public override object VisitMember(NameExpression node)
        {
            string name = node.Name;
            object target = null;
            Binders.IBinder binder = node.Binder;
            if (binder == null)
            {
                if (locals.TryFindVariable(name, out LocalVariable variable))
                {
                    if (variable.Type == null)
                        throw new Exception("value not initalized");
                    binder = new Binders.RuntimeVariableBinder(variable, locals);
                }
                else if (m_target != null && TypeExtensions.TryFindMember(m_target.GetType(), name, ReflectionUtils.AnyPublic, out binder))
                {
                    target = m_target;
                }
                else if (GlobalType.TryFindMember(name, ReflectionUtils.AnyPublic, out binder))
                {
                    target = m_global;
                }
                else if (TypeContext.TryGetType(name, out Type type))
                {
                    node.Type = type;
                    node.Binder = new Binders.EmptyBinder(type);
                    return null;
                }
                else
                {
                    var member = Resolver.Resolve(node) ?? throw ExecutionException.ThrowMissingMember(null, name, node);
                    node.Type = member.Type;
                    return member.Get(m_target);
                }
                node.Binder = binder;
                node.Type = binder.Type;
            }
            else if (binder is Binders.ITargetBinder targetBinder)
            {
                Type declaringType = targetBinder.DeclaringType;
                if (declaringType == m_target?.GetType())
                {
                    target = m_target;
                }
                else if (declaringType == GlobalType)
                {
                    target = m_global;
                }
                else
                {
                    ExecutionException.ThrowMissingMember(binder.Type, name, node);
                }
            }
            return binder.Get(target);
        }

        /// <inheritdoc/>
        void IStatementVisitor.VisitExpression(ExpressionStatement node)
        {
            node.Expression.Accept(this);
        }

        /// <inheritdoc/>
        public override object VisitDeclaration(VariableDeclarationExpression node)
        {
            object value = node.Value?.Accept(this);
            Type varType = value is null ? node.VariableType?.ResolveType(TypeContext) ?? TypeProvider.AnyType : value.GetType();
            locals.DeclareVariable(node.Name, varType, value);
            return value;
        }

        /// <inheritdoc/>
        void IStatementVisitor.VisitReturn(ReturnOrThrowStatement node)
        {
            var value = node.Value?.Accept(this);
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
            using (EnterScope())
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
                    using (EnterScope())
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
                    using (EnterScope())
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
                    using (EnterScope())
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
            if (VisitCondition(node.Condition))
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

        /// <inheritdoc/>
        void IStatementVisitor.VisitImport(ImportStatement node)
        {
            TypeContext.Register(node);
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

#if LATEST_VS
        readonly
#endif
        struct ScopedBranch : IDisposable
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
