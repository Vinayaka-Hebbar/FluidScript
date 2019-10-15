namespace FluidScript.SyntaxTree.Expressions
{
    public struct ArgumentExpression : IExpression
    {
        public readonly Object Value;

        public ArgumentExpression(Object value)
        {
            Value = value;
        }

        public Expression.Operation Kind => Expression.Operation.Argument;

        public TReturn Accept<TReturn>(INodeVisitor<TReturn> visitor) where TReturn : IRuntimeObject
        {
            return visitor.VisitArgument(this);
        }
    }
}
