namespace FluidScript.Compiler.SyntaxTree
{
    public interface IFunctionExpression
    {
        FunctionPartBuilder GetPartBuilder();
        Node[] Arguments { get; }
        Statement Body { get; }
    }
}
