namespace FluidScript.Compiler.SyntaxTree
{
    public class BlockStatement : Statement
    {
        //Todo Linq
        public readonly Statement[] Statements;
        public BlockStatement(Statement[] statements) : base(NodeType.Block)
        {
            Statements = statements;
        }

        public override TReturn Accept<TReturn>(INodeVisitor<TReturn> visitor)
        {
            return visitor.VisitBlock(this);
        }
    }
}
