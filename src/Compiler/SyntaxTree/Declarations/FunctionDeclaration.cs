using System.Linq;
using System.Reflection.Emit;

namespace FluidScript.Compiler.SyntaxTree
{
    public class FunctionDeclaration : Declaration
    {
        public readonly ArgumentInfo[] Arguments;

        public System.Type[] ArgumentTypes;

        public readonly Metadata.DeclarativeScope Scope;

        public FunctionDeclaration(string name, Emit.TypeName returnTypeName, ArgumentInfo[] arguments, Metadata.DeclarativeScope scope) : base(name, returnTypeName)
        {
            Arguments = arguments;
            Scope = scope;
        }

        protected override void TryResolveType(Emit.OptimizationInfo info)
        {

            base.TryResolveType(info);
            ArgumentTypes = Arguments.Select(arg => info.GetType(arg.TypeName)).ToArray();
        }

        internal MethodBuilder Declare(Reflection.DeclaredMethod method, TypeBuilder builder, Emit.OptimizationInfo info)
        {
            TryResolveType(info);
            return builder.DefineMethod(Name, System.Reflection.MethodAttributes.Public, ResolvedType, ArgumentTypes);
        }
    }
}
