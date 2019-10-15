namespace FluidScript
{
    public interface IExpression
    {
        Expression.Operation Kind { get; }
        TReturn Accept<TReturn>(INodeVisitor<TReturn> visitor) where TReturn : IRuntimeObject;
    }
}
