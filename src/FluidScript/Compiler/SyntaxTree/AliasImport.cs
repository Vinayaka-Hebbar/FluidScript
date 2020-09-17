namespace FluidScript.Compiler.SyntaxTree
{
    public class AliasImport : TypeImport
    {
        public readonly string Alias;

        public AliasImport(string alias, string typeSyntax) : base(typeSyntax)
        {
            Alias = alias;
        }

        public override string Name => Alias;

        public override string ToString()
        {
            return $"{Alias}={TypeName}";
        }
    }
}
