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
            if (ValueAtTop is SyntaxTree.TypeDefinitionStatement declaration)
            {
                var domain = System.Threading.Thread.GetDomain().DefineDynamicAssembly(new AssemblyName(assemblyName), System.Reflection.Emit.AssemblyBuilderAccess.RunAndSave);
                var module = domain.DefineDynamicModule(assemblyName);
                return declaration.Generate(module).CreateType();
            }
            return typeof(object);
        }

    }
}
