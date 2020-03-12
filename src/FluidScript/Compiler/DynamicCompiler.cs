using FluidScript.Compiler.SyntaxTree;
using FluidScript.Runtime;
using FluidScript.Utils;
using System;
using System.Collections.Generic;

namespace FluidScript.Compiler
{
    /// <summary>
    /// Runtime evaluation of Syntax tree with <see cref="Locals"/>
    /// <list type="bullet">it will be not same as compiled </list>
    /// </summary>
    public sealed class DynamicCompiler : CompilerBase, IEnumerable<KeyValuePair<string, object>>, IStatementVisitor
    {
        private readonly BranchContext context = new BranchContext();

        private readonly RuntimeVariables locals;

        public int Count => locals.Count;

        /// <summary>
        /// New runtime evaluation with <see cref="GlobalObject"/>
        /// </summary>
        public DynamicCompiler() : base(GlobalObject.Instance)
        {
            locals = new RuntimeVariables();
        }

        /// <summary>
        /// New runtime evaluation 
        /// </summary>
        public DynamicCompiler(object target) : base(target)
        {
            locals = new RuntimeVariables();
        }

        public DynamicCompiler(DynamicObject target) : base(target)
        {
            locals = new RuntimeVariables();
        }

        public DynamicCompiler(object target, IDictionary<string, object> locals) : base(target)
        {
            this.locals = new RuntimeVariables(locals);
        }

        /// <summary>
        /// New runtime evaluation 
        /// </summary>
        public DynamicCompiler(DynamicCompiler other) : base(other.Target)
        {
            locals = new RuntimeVariables(other.locals);
        }

        public IRuntimeVariables Locals
        {
            get
            {
                return locals;
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
            return expression.Accept(this);
        }

        #region Visitors

        /// <inheritdoc/>
        public override object VisitAssignment(AssignmentExpression node)
        {
            var value = node.Right.Accept(this);
            var left = node.Left;
            string name = null;
            object obj = Target;
            Binders.IBinder binder = null;
            ExpressionType nodeType = left.NodeType;
            Type type = node.Right.Type;
            if (nodeType == ExpressionType.Identifier)
            {
                var exp = (NameExpression)left;
                name = exp.Name;
                if (locals.TryLookVariable(name, out LocalVariable variable))
                {
                    locals.Update(variable, value);
                    node.Type = variable.Type;
                    return value;
                }
                else if (exp.Binder is null)
                {
                    binder = TypeUtils.GetMember(Target.GetType(), name);
                    exp.Binder = binder;
                }
                if (binder is null)
                {
                    //not found, add to global
                    locals.InsertAtRoot(name, node.Right.Type, value);
                    node.Type = type;
                    return value;
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
                if (obj is IMetaObjectProvider runtime)
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
                if (locals.TryLookVariable(name, out LocalVariable variable))
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
                    var type = obj.GetType();
                    var methods = TypeUtils.GetPublicMethods(type, name);
                    if (methods.Length == 0)
                        ExecutionException.ThrowMissingMethod(type, name, node);
                    method = TypeHelpers.BindToMethod(methods, args, out conversions);
                    exp.Binder = new Binders.FieldBinder(TargetField);
                    // exp.Type not resolved
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
                else if (obj is IMetaObjectProvider runtime)
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
            return obj;
        }
        #endregion


        /// <inheritdoc/>
        public override object VisitMember(MemberExpression node)
        {
            var target = node.Target.Accept(this);
            if (node.Binder == null)
            {
                var binder = TypeUtils.GetMember(node.Target.Type, node.Name);
                if (binder is null)
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
            }
            return node.Binder.Get(target);
        }

        /// <inheritdoc/>
        public override object VisitMember(NameExpression node)
        {
            string name = node.Name;
            object obj = Target;
            Binders.IBinder binder;
            if (node.Binder == null)
            {
                if (locals.TryLookVariable(name, out LocalVariable variable))
                {
                    if (variable.Type == null)
                        throw new Exception("value not initalized");
                    binder = new Binders.RuntimeVariableBinder(variable, locals);
                }
                else
                {
                    //find in the class level
                    binder = TypeUtils.GetMember(obj.GetType(), name);
                    if (binder is null)
                    {
                        node.Type = TypeProvider.ObjectType;
                        return null;
                    }
                }
                node.Binder = binder;
                node.Type = binder.Type;
            }
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
            locals.Create(node.Name, varType, value);
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
            using (locals.EnterScope())
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
                    using (locals.EnterScope())
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
                    using (locals.EnterScope())
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
                    using (locals.EnterScope())
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

        #region IEumerable

        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
        {
            return locals.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return locals.GetEnumerator();
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

        readonly struct ScopedBranch : IDisposable
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
