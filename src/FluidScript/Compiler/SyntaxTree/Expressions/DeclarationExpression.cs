namespace FluidScript.Compiler.SyntaxTree
{
    public class DeclarationExpression : Expression
    {
        public readonly string Name;

        protected DeclarationExpression(string name) : base(ExpressionType.Declaration)
        {
            Name = name;
        }
    }
}
