namespace FluidScript.Compiler.SyntaxTree
{
    public class BlockExpression : Expression
    {
        public readonly Statement[] Statements;
        public BlockExpression(Statement[] statements) : base(ExpressionType.Block)
        {
            Statements = statements;
        }

        public override TReturn Accept<TReturn>(INodeVisitor<TReturn> visitor)
        {
            return visitor.VisitBlock(this);
        }
    }
}
