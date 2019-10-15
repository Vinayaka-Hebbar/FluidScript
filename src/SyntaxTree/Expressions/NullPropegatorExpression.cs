namespace FluidScript.SyntaxTree.Expressions
{
    public class NullPropegatorExpression : Expression
    {
        public readonly IExpression Left;
        public readonly IExpression Right;

        public NullPropegatorExpression(IExpression left, IExpression right) : base(Operation.Invocation)
        {
            Left = left;
            Right = right;
        }

        public override TReturn Accept<TReturn>(INodeVisitor<TReturn> visitor)
        {
            return visitor.VisitNullPropagator(this);
        }
    }
}
