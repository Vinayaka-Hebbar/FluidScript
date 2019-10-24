namespace FluidScript.Compiler.SyntaxTree
{
    public class SyntaxExpression : Expression
    {
        public SyntaxExpression(NodeType opCode) : base(opCode)
        {
        }

        public override TReturn Accept<TReturn>(INodeVisitor<TReturn> visitor)
        {
            return visitor.VisitVoid();
        }
    }
}
