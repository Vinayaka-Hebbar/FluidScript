namespace FluidScript.Compiler.SyntaxTree
{
    public sealed class AnonymousObjectMember : Node
    {
        public readonly string Name;
        public readonly Expression Expression;

        public AnonymousObjectMember(string name, Expression expression)
        {
            Name = name;
            Expression = expression;
        }

        public override string ToString()
        {
            return string.Concat(Name, ":", Expression);
        }
    }
}
