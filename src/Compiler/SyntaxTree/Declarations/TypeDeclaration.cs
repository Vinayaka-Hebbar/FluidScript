
namespace FluidScript.Compiler.SyntaxTree
{
    public class TypeDeclaration : Declaration
    {
        public readonly string BaseTypeName;
        public readonly string[] Implements;
        public readonly Scopes.ObjectScope Scope;
        public TypeDeclaration(string name, string baseTypeName, string[] implements, Scopes.ObjectScope scope) : base(name)
        {
            BaseTypeName = baseTypeName;
            Implements = implements;
            Scope = scope;
        }

        internal System.Reflection.Emit.TypeBuilder Generate(System.Reflection.Emit.ModuleBuilder builder)
        {
            var scope = Scope;
            var typeBuilder = builder.DefineType(Name, System.Reflection.TypeAttributes.Public, BaseTypeName == null ? null : builder.GetType(BaseTypeName));
            var typeProvider = new Emit.TypeProvider((name, throwOnError) =>
            {
                if (Emit.TypeUtils.PrimitiveNames.ContainsKey(name))
                    return Emit.TypeUtils.PrimitiveNames[name].Type;
                return builder.GetType(name, throwOnError);
            });
            foreach (var member in scope.Members)
            {
                switch (member.MemberType)
                {
                    case System.Reflection.MemberTypes.Method:
                        var method = (Reflection.DeclaredMethod)member;
                        if (method.Declaration is FunctionDeclaration declaration)
                        {
                            method.Store = declaration.Declare(member, typeBuilder, typeProvider);
                        }
                        break;
                }
            }

            foreach (var member in scope.Members)
            {
                switch (member.MemberType)
                {
                    case System.Reflection.MemberTypes.Method:
                        var method = (Reflection.DeclaredMethod)member;
                        member.Generate(typeProvider);
                        break;
                }
            }
            return typeBuilder;
        }

        internal System.Type Create(string assemblyName)
        {
            var domain = System.Threading.Thread.GetDomain().DefineDynamicAssembly(new System.Reflection.AssemblyName(assemblyName), System.Reflection.Emit.AssemblyBuilderAccess.RunAndSave);
            var module = domain.DefineDynamicModule(assemblyName);
            return Generate(module).CreateType();
        }
    }
}
