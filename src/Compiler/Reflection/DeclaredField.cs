using System.Reflection;

namespace FluidScript.Compiler.Reflection
{
    internal sealed class DeclaredField : DeclaredMember
    {
        public System.Reflection.Emit.FieldBuilder Store;
        public DeclaredField(SyntaxTree.Declaration declaration, int index, BindingFlags binding) : base(declaration, index, binding, System.Reflection.MemberTypes.Field)
        {
        }

        public override MemberInfo Memeber => Store;
    }
}
