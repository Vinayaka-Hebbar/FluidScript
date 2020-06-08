using FluidScript.Compiler.SyntaxTree;
using FluidScript.Runtime;
using FluidScript.Utils;
using System;

namespace FluidScript.Compiler
{
    /// <summary>
    /// Runtime evaluation of Syntax tree with <see cref="Locals"/>
    /// <list type="bullet">it will be not same as compiled </list>
    /// </summary>
    public sealed class RuntimeCompiler : CompilerBase, IStatementVisitor, ICompileProvider
    {
        readonly BranchContext context = new BranchContext();
        readonly RuntimeVariables locals;

        readonly object m_global;
        object m_target;
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

        static System.Reflection.FieldInfo gField;
        static System.Reflection.FieldInfo GlobalTargetField
        {
            get
            {
                if (gField == null)
                    gField = typeof(RuntimeCompiler).GetField(nameof(m_global), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                return gField;
            }
        }

        static System.Reflection.FieldInfo cField;
        static System.Reflection.FieldInfo ClassTargetField
        {
            get
            {
                if (cField == null)
                    cField = typeof(RuntimeCompiler).GetField(nameof(m_target), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                return cField;
            }
        }



        Type globalType;
        public Type GlobalType
        {
            get
            {
                if (globalType == null)
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
        public object Invoke(Statement statement, object target = null)
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
        public object Invoke(Expression expression, object target = null)
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
                    if (m_target != null && TypeUtils.TryFindMember(m_target.GetType(), name, TypeUtils.AnyPublic, out binder))
                    {
                        exp.Target = m_target;
                    }
                    else if (TypeUtils.TryFindMember(GlobalType, name, TypeUtils.AnyPublic, out binder))
                    {
                        exp.Target = m_global;
                    }
                    else
                    {
                        //not found, add to global
                        locals.InsertAtRoot(name, node.Right.Type, value);
                        node.Type = type;
                        return value;
                    }
                    exp.Binder = binder;
                }
                obj = exp.Target;
            }
            else if (nodeType == ExpressionType.MemberAccess)
            {
                var exp = (MemberExpression)left;
                obj = exp.Target.Accept(this);
                name = exp.Name;
                if (binder is null && TypeUtils.TryFindMember(exp.Target.Type, name, TypeUtils.AnyPublic, out binder))
                {
                    exp.Binder = binder;
                }
                else if (obj is IMetaObjectProvider runtime)
                {
                    //todo binder for dynamic
                    var result = runtime.GetMetaObject().BindSetMember(name, node.Right.Type, value).Value;
                    value = result.Value;
                    node.Type = result.Type;
                    return value;
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
                if (TypeUtils.TryImplicitConvert(type, binder.Type, out System.Reflection.MethodInfo method))
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
            Binders.ArgumentConversions conversions = null;
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
                if (locals.TryFindVariable(name, out LocalVariable variable))
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
                    obj = Target;
                    var type = GlobalType;
                    if (m_target != null && TypeHelpers.TryFindMethod(name, m_target.GetType(), args, out method, out conversions))
                        exp.Binder = new Binders.FieldBinder(ClassTargetField);
                    else if (TypeHelpers.TryFindMethod(name, type, args, out method, out conversions))
                        exp.Binder = new Binders.FieldBinder(GlobalTargetField);
                    else
                        ExecutionException.ThrowMissingMethod(type, name, node);
                    // exp.Type not resolved
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
        #endregion

        /// <inheritdoc/>
        public override object VisitMember(MemberExpression node)
        {
            var target = node.Target.Accept(this);
            if (node.Binder == null)
            {
                if (TypeUtils.TryFindMember(node.Target.Type, node.Name, TypeUtils.AnyPublic, out Binders.IBinder binder) == false
                    && target is IMetaObjectProvider runtime)
                {
                    binder = runtime.GetMetaObject().BindGetMember(node.Name);
                    if (binder is null)
                    {
                        node.Type = TypeProvider.ObjectType;
                        return null;
                    }
                }
                if (binder is null)
                    // target null member cannot be invoked
                    ExecutionException.ThrowNullError(node);
                node.Binder = binder;
                node.Type = binder.Type;
            }
            return node.Binder.Get(target);
        }

        /// <inheritdoc/>
        public override object VisitMember(NameExpression node)
        {
            string name = node.Name;
            if (node.Binder == null)
            {
                Binders.IBinder binder;
                if (locals.TryFindVariable(name, out LocalVariable variable))
                {
                    if (variable.Type == null)
                        throw new Exception("value not initalized");
                    binder = new Binders.RuntimeVariableBinder(variable, locals);
                    goto done;
                }
                else if (m_target != null)
                {
                    var target = m_target;
                    if (TypeUtils.TryFindMember(target.GetType(), name, TypeUtils.AnyPublic, out binder))
                    {
                        node.Target = target;
                        goto done;
                    }
                }
                if (TypeUtils.TryFindMember(GlobalType, name, TypeUtils.AnyPublic, out binder))
                {
                    node.Target = m_global;
                }
                else
                {
                    node.Type = TypeProvider.ObjectType;
                    return null;
                }
            done:
                node.Binder = binder;
                node.Type = binder.Type;
            }
            var obj = node.Target;
            return node.Binder.Get(obj);
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
            Type varType = value is null ? node.VariableType?.GetType(TypeProvider.Default) ?? TypeProvider.ObjectType : value.GetType();
            locals.DeclareVariable(node.Name, varType, value);
            return value;
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
