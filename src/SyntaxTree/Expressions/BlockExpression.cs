namespace FluidScript.SyntaxTree.Expressions
{
    public class BlockExpression : Expression
    {
        public readonly Statement[] Statements;
        public BlockExpression(Statement[] statements) : base(Operation.Block)
        {
            Statements = statements;
        }

        public override TReturn Accept<TReturn>(INodeVisitor<TReturn> visitor)
        {
            return visitor.VisitBlock(this);
        }
    }
}
