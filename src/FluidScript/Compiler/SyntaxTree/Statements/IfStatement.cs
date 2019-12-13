namespace FluidScript.Compiler.SyntaxTree
{
    /// <summary>
    /// If condition syntax 
    /// </summary>
    public sealed class IfStatement : Statement
    {
        /// <summary>
        /// Condition
        /// </summary>
        public readonly Expression Condition;
        /// <summary>
        /// True
        /// </summary>
        public readonly Statement Then;
        /// <summary>
        /// False
        /// </summary>
        public readonly Statement Other;

        /// <summary>
        /// Initializes new <see cref="IfStatement"/>
        /// </summary>
        public IfStatement(Expression condition, Statement then, Statement other) : base(StatementType.If)
        {
            Condition = condition;
            Then = then;
            Other = other;
        }

        /// <inheritdoc/>
        protected internal override void Accept(IStatementVisitor visitor)
        {
            visitor.VisitIf(this);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            var elseCondition = Other == null ? string.Empty : string.Concat("else ", Other);
            return string.Concat("(", Condition, ")", Then, "\n", elseCondition);
        }
    }
}
