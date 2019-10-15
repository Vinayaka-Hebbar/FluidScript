namespace FluidScript.SyntaxTree.Expressions
{
    public class SyntaxExpression : IExpression
    {
        public SyntaxExpression(Expression.Operation opCode)
        {
            Kind = opCode;
        }

        public Expression.Operation Kind { get; }

        public TReturn Accept<TReturn>(INodeVisitor<TReturn> visitor) where TReturn : IRuntimeObject
        {
            return visitor.VisitVoid();
        }
    }
}
