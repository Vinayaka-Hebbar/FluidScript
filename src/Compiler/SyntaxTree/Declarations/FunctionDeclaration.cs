using System.Linq;
using System.Reflection.Emit;

namespace FluidScript.Compiler.SyntaxTree
{
    public class FunctionDeclaration : Declaration
    {
        public readonly ArgumentInfo[] Arguments;

        public readonly string ReturnTypeName;
        public FunctionDeclaration(string name, string returnTypeName, ArgumentInfo[] arguments) : base(name)
        {
            ReturnTypeName = returnTypeName;
            Arguments = arguments;
        }

        public System.Type[] ArgumentTypes(Emit.TypeProvider provider)
        {
            return Arguments.Select(arg => provider.GetType(arg.TypeName)).ToArray();
        }

        public System.Type ReturnType(Emit.TypeProvider provider)
        {
            return provider.GetType(ReturnTypeName);
        }

        internal MethodBuilder Declare(Reflection.DeclaredMember member, TypeBuilder typeBuilder, Emit.TypeProvider typeProvider)
        {
            return typeBuilder.DefineMethod(Name, System.Reflection.MethodAttributes.Public, ReturnType(typeProvider), ArgumentTypes(typeProvider));
        }
    }
}
