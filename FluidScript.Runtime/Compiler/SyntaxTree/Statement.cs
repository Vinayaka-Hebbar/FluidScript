namespace FluidScript.Compiler.SyntaxTree
{
    public abstract class Statement : Node
    {
        private static readonly string[] NoLabels = new string[0];
        internal static readonly Statement Empty = new EmptyStatement();
        public readonly string[] Labels;

        protected Statement(StatementType nodeType)
        {
            Labels = NoLabels;
            NodeType = nodeType;
        }

        protected Statement(string[] labels, StatementType nodeType)
        {
            Labels = labels;
            NodeType = nodeType;
        }

        public TextSpan Span
        {
            get;
            set;
        }

        public StatementType NodeType { get; }

        public bool HasLabels => Labels.Length > 0;

        protected internal virtual void Accept(IStatementVisitor visitor)
        {

        }

        public virtual void GenerateCode(Reflection.Emit.MethodBodyGenerator generator)
        {
            generator.NoOperation();
        }

        public class StatementLocals
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
            public Reflection.Emit.ILLabel EndOfStatement;

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
        /// <param name="generator"> The generator to output the CIL to. </param>
        /// <param name="info"> Information about any optimizations that should be performed. </param>
        /// <param name="locals"> Variables common to both GenerateStartOfStatement() and GenerateEndOfStatement(). </param>
        protected void GenerateStartOfStatement(Reflection.Emit.MethodBodyGenerator generator, StatementLocals locals)
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
        /// <param name="method"> Information about any optimizations that should be performed. </param>
        /// <param name="locals"> Variables common to both GenerateStartOfStatement() and GenerateEndOfStatement(). </param>
        protected void GenerateEndOfStatement(Reflection.Emit.MethodBodyGenerator generator, StatementLocals locals)
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

    }

    internal class EmptyStatement : Statement
    {
        public EmptyStatement() : base(StatementType.Empty)
        {
        }

    }
}
