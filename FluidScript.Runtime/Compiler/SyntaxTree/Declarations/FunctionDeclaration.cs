using System.Linq;

namespace FluidScript.Compiler.SyntaxTree
{
    public class FunctionDeclaration : Declaration
    {
        public readonly ArgumentInfo[] Arguments;

        public readonly Metadata.FunctionPrototype Scope;

        public FunctionDeclaration(string name, Emit.TypeName returnTypeName, ArgumentInfo[] arguments, Metadata.FunctionPrototype scope) : base(name, returnTypeName)
        {
            Arguments = arguments;
            Scope = scope;
        }

        public System.Collections.Generic.IEnumerable<PrimitiveType> PrimitiveArguments()
        {
            foreach (var arg in Arguments)
            {
                yield return Emit.TypeUtils.GetPrimitiveType(arg.TypeName);
            }
        }

        public System.Collections.Generic.IEnumerable<System.Type> ArgumentTypes()
        {
            return Arguments.Select(arg => Emit.TypeUtils.GetType(arg.TypeName));
        }
    }
}
