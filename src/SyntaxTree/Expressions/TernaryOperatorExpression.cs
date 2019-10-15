namespace FluidScript.SyntaxTree.Expressions
{
    public class TernaryOperatorExpression : Expression
    {
        public readonly IExpression First;

        public readonly IExpression Second;

        public readonly IExpression Third;

        public TernaryOperatorExpression(IExpression first, IExpression second, IExpression third) : base(Operation.Question)
        {
            First = first;
            Second = second;
            Third = third;
        }

        public override TReturn Accept<TReturn>(INodeVisitor<TReturn> visitor)
        {
            return First.Accept(visitor).ToBool() ? Second.Accept(visitor) : Third.Accept(visitor);
        }
    }
}
