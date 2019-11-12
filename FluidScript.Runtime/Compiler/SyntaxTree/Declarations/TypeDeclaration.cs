using System.Linq;

namespace FluidScript.Compiler.SyntaxTree
{
    public class TypeDeclaration : Declaration
    {
        public readonly string[] Implements;
        public readonly Metadata.ObjectPrototype Scope;
        public TypeDeclaration(string name, string baseTypeName, string[] implements, Metadata.ObjectPrototype scope) : base(name)
        {
            TypeName = new Emit.TypeName(baseTypeName);
            Implements = implements;
            Scope = scope;
        }

        public override Emit.TypeName TypeName { get; }

#if Emit
        internal System.Reflection.Emit.TypeBuilder Declare(System.Reflection.Emit.ModuleBuilder builder, Emit.OptimizationInfo info)
        {
            var scope = Scope;
            if (ResolvedType == null)
            {
                TryResolveType(info);
            }
            var typeBuilder = builder.DefineType(Name, System.Reflection.TypeAttributes.Public, ResolvedType);
            info.DeclaringType = typeBuilder;
            foreach (var member in scope.Members)
            {
                switch (member.MemberType)
                {
                    case System.Reflection.MemberTypes.Method:
                        var method = (Reflection.DeclaredMethod)member;
                        if (method.Declaration is FunctionDeclaration declaration)
                        {
                            method.Store = declaration.Declare(method, typeBuilder, info);
                        }
                        break;
                    case System.Reflection.MemberTypes.Field:
                        var field = (Reflection.DeclaredField)member;
                        if (member.Declaration is FieldDelcaration delcaration)
                        {
                            field.Store = delcaration.Declare(field, typeBuilder, info);
                        }
                        break;
                }
            }
            if (!scope.Members.Any(memeber => memeber.MemberType == System.Reflection.MemberTypes.Constructor))
            {
                //Initialize
                var initializer = typeBuilder.DefineConstructor(System.Reflection.MethodAttributes.Public, System.Reflection.CallingConventions.Standard, new System.Type[0]);
                var generator = new Emit.ReflectionILGenerator(initializer.GetILGenerator(), false);
                var constructorInfo = new Emit.MethodOptimizationInfo(info)
                {
                    DeclaringType = typeBuilder
                };
                generator.LoadArgument(0);
                var ctor = typeBuilder.BaseType.GetConstructor(new System.Type[0]);
                generator.Call(ctor);
                foreach (Reflection.DeclaredMember field in scope.Fields)
                {
                    field.Generate(generator, constructorInfo);
                }
                generator.Complete();
            }
            foreach (var member in scope.Members)
            {
                if (member.MemberType == System.Reflection.MemberTypes.Method || member.MemberType == System.Reflection.MemberTypes.Property)
                {
                    member.Generate(info);
                }
            }
            return typeBuilder;
        }

        internal System.Type Create(string assemblyName)
        {
            var domain = System.Threading.Thread.GetDomain().DefineDynamicAssembly(new System.Reflection.AssemblyName(assemblyName), System.Reflection.Emit.AssemblyBuilderAccess.RunAndSave);
            var module = domain.DefineDynamicModule(assemblyName);
            var moduleInfo = new Emit.OptimizationInfo(domain.GetType);
            System.Reflection.Emit.TypeBuilder typeBuilder = Declare(module, moduleInfo);
            return typeBuilder.CreateType();
        }

#endif
    }
}
