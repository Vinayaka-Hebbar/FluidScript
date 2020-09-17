using FluidScript.Compiler.Emit;

namespace FluidScript.Compiler.SyntaxTree
{
    public class TypeImport : Node
    {
        public readonly TypeName TypeName;

        public TypeImport(string typeSyntax)
        {
            TypeName = typeSyntax;
        }

        public virtual string Name => TypeName.Name;

        public override string ToString()
        {
            return TypeName.FullName;
        }
    }
}
