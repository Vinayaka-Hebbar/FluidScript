namespace FluidScript.SyntaxTree.Expressions
{
    public class LiteralExpression : Expression
    {
        public LiteralExpression(double value) : base(Operation.Numeric)
        {
            Value = new Object(value);
        }

        public LiteralExpression(string value) : base(Operation.String)
        {
            Value = new Object(value);
        }

        public LiteralExpression(Object value) : base(Operation.Literal)
        {
            Value = value;
        }

        public readonly Object Value;

        public override TReturn Accept<TReturn>(INodeVisitor<TReturn> visitor)
        {
            return visitor.VisitLiteral(this);
        }
    }
}
