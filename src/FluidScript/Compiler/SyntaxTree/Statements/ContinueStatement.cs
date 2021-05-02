namespace FluidScript.Compiler.SyntaxTree
{
    /// <summary>
    /// Represents a continue statement.
    /// </summary>
    public sealed class ContinueStatement : Statement
    {
        /// <summary>
        /// Gets or sets the name of the label that identifies the loop to continue.  Can be <c>null</c>
        /// </summary>
        public readonly string Label;

        /// <summary>
        /// Creates a new ContinueStatement instance.
        /// </summary>
        public ContinueStatement(string label) : base(StatementType.Continue)
        {
            Label = label;
        }

        /// <inheritdoc/>
        protected internal override void Accept(IStatementVisitor visitor)
        {
            visitor.VisitContinue(this);
        }

        /// <inheritdoc/>
        public override void GenerateCode(Compiler.Emit.MethodBodyGenerator generator)
        {
            // Generate code for the start of the statement.
            var statementLocals = new StatementLocals();
            GenerateStartOfStatement(generator, statementLocals);

            // Emit an unconditional branch.
            // Note: the break statement might be branching from inside a try { } or finally { }
            // block to outside.  EmitLongJump() handles this.
            generator.EmitLongJump(generator, generator.GetContinueTarget(Label));

            // Generate code for the end of the statement.
            GenerateEndOfStatement(generator, statementLocals);
        }

        public override string ToString()
        {
            return Label != null ? string.Concat("continue ", Label) : string.Concat("continue");
        }

    }
}
