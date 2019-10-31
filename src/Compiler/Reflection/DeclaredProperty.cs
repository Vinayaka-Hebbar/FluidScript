using System.Reflection;

namespace FluidScript.Compiler.Reflection
{
    internal sealed class DeclaredProperty : DeclaredMember
    {
        public System.Reflection.Emit.PropertyBuilder Store;
        public DeclaredProperty(SyntaxTree.Declaration declaration, int index , BindingFlags binding) : base(declaration, index, binding, System.Reflection.MemberTypes.Property)
        {
        }

        public override MemberInfo Memeber => Store;
    }
}
