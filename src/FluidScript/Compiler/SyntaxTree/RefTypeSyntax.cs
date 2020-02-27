namespace FluidScript.Compiler.SyntaxTree
{
    public class RefTypeSyntax : TypeSyntax
    {
        public readonly string Name;

        public RefTypeSyntax(string name)
        {
            Name = name;
        }

        public override System.Type GetType(ITypeProvider provider)
        {
            return provider.GetType(Name);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
