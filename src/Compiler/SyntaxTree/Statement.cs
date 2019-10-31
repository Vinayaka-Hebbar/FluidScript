using FluidScript.Compiler.Emit;

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

        public StatementType NodeType { get; }

        public bool HasLabels => Labels.Length > 0;

        public override object GetValue()
        {
            return null;
        }

        public override void GenerateCode(ILGenerator generator, OptimizationInfo info)
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
            public ILLabel EndOfStatement;

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
        public void GenerateStartOfStatement(ILGenerator generator, OptimizationInfo info, StatementLocals locals)
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
                info.PushBreakOrContinueInfo(this.Labels, locals.EndOfStatement, null, labelledOnly: true);
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
        /// <param name="info"> Information about any optimizations that should be performed. </param>
        /// <param name="locals"> Variables common to both GenerateStartOfStatement() and GenerateEndOfStatement(). </param>
        public void GenerateEndOfStatement(ILGenerator generator, OptimizationInfo info, StatementLocals locals)
        {
            if (locals.NonDefaultBreakStatementBehavior == false && this.HasLabels == true)
            {
                // Revert the information needed by the break statement.
                generator.DefineLabelPosition(locals.EndOfStatement);
                info.PopBreakOrContinueInfo();
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
