namespace FluidScript.Compiler.SyntaxTree
{
    public abstract class TypeSyntax : Node
    {
        public abstract System.Type GetType(ITypeProvider provider);
    }
}
