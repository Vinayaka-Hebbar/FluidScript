namespace FluidScript.Compiler.SyntaxTree
{
    /// <summary>
    /// Represents a break statement.
    /// </summary>
    public sealed class BreakStatement : Statement
    {
        /// <summary>
        /// Creates a new BreakStatement instance.
        /// </summary>
        public BreakStatement()
            : base(StatementType.Break)
        {
        }

        /// <inheritdoc/>
        protected internal override void Accept(IStatementVisitor visitor)
        {
            visitor.VisitBreak(this);
        }

        /// <inheritdoc/>
        public override void GenerateCode(Reflection.Emit.MethodBodyGenerator generator)
        {
            // Generate code for the start of the statement.
            var statementLocals = new StatementLocals();
            GenerateStartOfStatement(generator, statementLocals);

            // Emit an unconditional branch.
            // Note: the break statement might be branching from inside a try { } or finally { }
            // block to outside.  EmitLongJump() handles this.
            generator.EmitLongJump(generator, generator.GetBreakTarget());

            // Generate code for the end of the statement.
            GenerateEndOfStatement(generator, statementLocals);
        }

        public override string ToString()
        {
            return "break";
        }
    }
}
