namespace FluidScript.Compiler.SyntaxTree
{
    /// <summary>
    /// Statement like declarion, return etc
    /// </summary>
    public abstract class Statement : Node
    {
        private static readonly string[] NoLabels = new string[0];

        internal static readonly Statement Empty = new EmptyStatement();

        internal static readonly Statement Break = new BreakStatement();
        /// <summary>
        /// Labels associated with the statement
        /// </summary>
        public readonly string[] Labels;

        /// <summary>
        /// Initializes new <see cref="Statement"/>
        /// </summary>
        protected Statement(StatementType nodeType)
        {
            Labels = NoLabels;
            NodeType = nodeType;
        }

        /// <summary>
        /// Initializes new <see cref="Statement"/>
        /// </summary>
        protected Statement(string[] labels, StatementType nodeType)
        {
            Labels = labels;
            NodeType = nodeType;
        }

        /// <summary>
        /// Statement Position
        /// </summary>
        public Debugging.TextSpan Span
        {
            get;
            set;
        }

        /// <summary>
        /// Node Type
        /// </summary>
        public StatementType NodeType { get; }

        /// <summary>
        /// Indicates whether statement has any label
        /// </summary>
        public bool HasLabels => Labels.Length > 0;

        /// <summary>
        /// Optimizes or evaluetes <see cref="Statement"/>
        /// </summary>
        protected internal virtual void Accept(IStatementVisitor visitor)
        {

        }

        /// <summary>
        /// Creates IL code 
        /// </summary>
        /// <param name="generator">The generator to output the CIL to.</param>
        public virtual void GenerateCode(Emit.MethodBodyGenerator generator)
        {
            generator.NoOperation();
        }

        /// <summary>
        /// Statement Locals
        /// </summary>
        /// 
        public sealed class StatementLocals
        {
            /// <summary>
            /// Gets or sets a value that indicates whether the break statement will be handled
            /// specially by the calling code - this means that GenerateStartOfStatement() and
            /// GenerateEndOfStatement() do not have to generate code to handle the break
            /// statement.
            /// </summary>
            public bool NonDefaultBreakStatementBehavior;

            /// <summary>
            /// Gets or sets a value that indicates whether the debugging information will be
            /// handled specially by the calling code - this means that GenerateStartOfStatement()
            /// and GenerateEndOfStatement() do not have to set this information.
            /// </summary>
            public bool NonDefaultSourceSpanBehavior;

            /// <summary>
            /// Gets or sets a label marking the end of the statement.
            /// </summary>
            public Compiler.Emit.ILLabel EndOfStatement;

#if DEBUG
            /// <summary>
            /// Gets or sets the number of items on the IL stack at the start of the statement.
            /// </summary>
            public int OriginalStackSize;
#endif
        }

        /// <summary>
        /// Generates CIL for the start of every statement.
        /// </summary>
        /// <param name="generator">The generator to output the CIL to. </param>
        /// <param name="locals"> Variables common to both GenerateStartOfStatement() and GenerateEndOfStatement(). </param>
        protected void GenerateStartOfStatement(Compiler.Emit.MethodBodyGenerator generator, StatementLocals locals)
        {
#if DEBUG && USE_DYNAMIC_IL_INFO
            // Statements must not produce or consume any values on the stack.
            if (generator is DynamicILGenerator)
                locals.OriginalStackSize = ((DynamicILGenerator)generator).StackSize;
#endif

            if (locals.NonDefaultBreakStatementBehavior == false && this.HasLabels == true)
            {
                // Set up the information needed by the break statement.
                locals.EndOfStatement = generator.CreateLabel();
                generator.PushBreakOrContinueInfo(this.Labels, locals.EndOfStatement, null, labelledOnly: true);
            }

            // Emit debugging information.
            if (locals.NonDefaultSourceSpanBehavior == false)
            {
                //todo span
                // optimizationInfo.MarkSequencePoint(generator, this.SourceSpan);
            }
        }

        /// <summary>
        /// Generates CIL for the end of every statement.
        /// </summary>
        /// <param name="generator"> The generator to output the CIL to. </param>
        /// <param name="locals"> Variables common to both GenerateStartOfStatement() and GenerateEndOfStatement(). </param>
        protected void GenerateEndOfStatement(Compiler.Emit.MethodBodyGenerator generator, StatementLocals locals)
        {
            if (locals.NonDefaultBreakStatementBehavior == false && this.HasLabels == true)
            {
                // Revert the information needed by the break statement.
                generator.DefineLabelPosition(locals.EndOfStatement);
                generator.PopBreakOrContinueInfo();
            }

#if DEBUG && USE_DYNAMIC_IL_INFO
            // Check that the stack count is zero.
            if (generator is DynamicILGenerator && ((DynamicILGenerator)generator).StackSize != locals.OriginalStackSize)
                throw new InvalidOperationException("Encountered unexpected stack imbalance.");
#endif
        }

        #region Static
        public static ReturnOrThrowStatement Return(Expression value = null)
        {
            return new ReturnOrThrowStatement(value, StatementType.Return);
        }

        public static Statement Block(params Statement[] statements)
        {
            return new BlockStatement(new NodeList<Statement>(statements));
        }

        public static IfStatement If(Expression condition, Statement then, Statement other = null)
        {
            return new IfStatement(condition, then, other);
        }
        #endregion
    }

    internal class EmptyStatement : Statement
    {
        public EmptyStatement() : base(StatementType.Empty)
        {
        }

        public override string ToString()
        {
            return string.Empty;
        }
    }
}
