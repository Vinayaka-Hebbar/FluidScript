namespace FluidScript.Compiler.SyntaxTree
{
    public class TypeImport : Node
    {
        public readonly string Name;

        public TypeImport(string typeSyntax)
        {
            Name = typeSyntax;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
