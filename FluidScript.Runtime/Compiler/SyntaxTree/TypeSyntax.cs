namespace FluidScript.Compiler.SyntaxTree
{
    public abstract class TypeSyntax : Node
    {
        public abstract Reflection.ITypeInfo GetTypeInfo();
    }
}
