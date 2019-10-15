namespace FluidScript.SyntaxTree.Statements
{
    public class ReturnOrThrowStatement : Statement
    {
        public readonly IExpression Expression;
        public ReturnOrThrowStatement(Operation opCode, IExpression expression) : base(opCode)
        {
            Expression = expression;
        }

        public override TReturn Accept<TReturn>(INodeVisitor<TReturn> visitor)
        {
            return visitor.VisitReturnOrThrow(this);
        }
    }
}
