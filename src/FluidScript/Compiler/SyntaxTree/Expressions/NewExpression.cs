namespace FluidScript.Compiler.SyntaxTree
{
    public class NewExpression : Expression
    {
        public readonly string Name;
        public readonly Expression[] Arguments;

        public NewExpression(string name, Expression[] arguments) : base(ExpressionType.New)
        {
            Name = name;
            Arguments = arguments;
        }
    }
}
