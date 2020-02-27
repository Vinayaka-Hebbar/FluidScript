namespace FluidScript.Compiler.SyntaxTree
{
    public class NewExpression : Expression
    {
        public readonly string Name;
        public readonly NodeList<Expression> Arguments;

        public NewExpression(string name, NodeList<Expression> arguments) : base(ExpressionType.New)
        {
            Name = name;
            Arguments = arguments;
        }
    }
}
