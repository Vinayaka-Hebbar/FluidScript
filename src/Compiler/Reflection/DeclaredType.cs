using System.Reflection;

namespace FluidScript.Compiler.Reflection
{
    public sealed class DeclaredType : DeclaredMember
    {
        public System.Type Store;
        public DeclaredType(SyntaxTree.Declaration declaration, int index, BindingFlags binding) : base(declaration, index, binding, System.Reflection.MemberTypes.TypeInfo)
        {
        }

        public override MemberInfo Memeber => Store;

        public System.Type Create(string assemblyName)
        {
            if (Declaration is SyntaxTree.TypeDeclaration declaration)
            {
                return declaration.Create(assemblyName);
            }
            return typeof(object);
        }

    }
}
