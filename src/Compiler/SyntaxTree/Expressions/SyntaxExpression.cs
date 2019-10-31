namespace FluidScript.Compiler.SyntaxTree
{
    public class SyntaxExpression : Expression
    {
        public SyntaxExpression(ExpressionType opCode) : base(opCode)
        {
        }

        public override TReturn Accept<TReturn>(INodeVisitor<TReturn> visitor)
        {
            return visitor.VisitVoid();
        }
    }
}
