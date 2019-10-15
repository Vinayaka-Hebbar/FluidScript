namespace FluidScript
{
    public interface IFunctionExpression
    {
        FunctionPartBuilder GetPartBuilder();
        IExpression[] Arguments { get; }
        Statement Body { get; }
    }
}
