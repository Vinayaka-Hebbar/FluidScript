namespace FluidScript.SyntaxTree.Statements
{
    public sealed class IfStatement : Statement
    {
        public readonly IExpression Expression;
        public readonly Statement Body;
        public readonly Statement Other;

        public IfStatement(IExpression expression, Statement body, Statement other) : base(Operation.If)
        {
            Expression = expression;
            Body = body;
            Other = other;
        }

        public override TReturn Accept<TReturn>(INodeVisitor<TReturn> visitor)
        {
            return visitor.VisitIfElse(this);
        }
    }
}
