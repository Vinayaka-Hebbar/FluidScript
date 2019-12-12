namespace FluidScript.Compiler.SyntaxTree
{
    public sealed class NullPropegatorExpression : Expression
    {
        public readonly Expression Left;
        public readonly Expression Right;

        public NullPropegatorExpression(Expression left, Expression right) : base(ExpressionType.Invocation)
        {
            Left = left;
            Right = right;
        }

        public override TResult Accept<TResult>(IExpressionVisitor<TResult> visitor)
        {
            return visitor.VisitNullPropegator(this);
        }
    }
}
