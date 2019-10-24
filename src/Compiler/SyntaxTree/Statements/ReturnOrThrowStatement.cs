namespace FluidScript.Compiler.SyntaxTree
{
    public class ReturnOrThrowStatement : Statement
    {
        public readonly Expression Expression;
        public ReturnOrThrowStatement(NodeType opCode, Expression expression) : base(opCode)
        {
            Expression = expression;
        }

        public override TReturn Accept<TReturn>(INodeVisitor<TReturn> visitor)
        {
            return visitor.VisitReturnOrThrow(this);
        }
    }
}
