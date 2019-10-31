namespace FluidScript.Compiler.SyntaxTree
{
    public sealed class IfStatement : Statement
    {
        public readonly Expression Expression;
        public readonly Statement Body;
        public readonly Statement Other;

        public IfStatement(Expression expression, Statement body, Statement other) : base(StatementType.If)
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
