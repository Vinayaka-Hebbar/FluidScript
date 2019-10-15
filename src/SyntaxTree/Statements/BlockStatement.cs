namespace FluidScript.SyntaxTree.Statements
{
    public class BlockStatement : Statement
    {
        //Todo Linq
        public readonly Statement[] Statements;
        public BlockStatement(Statement[] statements) : base(Operation.Block)
        {
            Statements = statements;
        }

        public override TReturn Accept<TReturn>(INodeVisitor<TReturn> visitor)
        {
            return visitor.VisitBlock(this);
        }
    }
}
