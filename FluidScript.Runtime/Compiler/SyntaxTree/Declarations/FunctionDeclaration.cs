using System.Linq;

namespace FluidScript.Compiler.SyntaxTree
{
    public class FunctionDeclaration : Declaration
    {
        public readonly ArgumentInfo[] Arguments;

        public readonly Metadata.FunctionPrototype Prototype;

        public FunctionDeclaration(string name, Emit.TypeName returnTypeName, ArgumentInfo[] arguments, Metadata.FunctionPrototype prototype) : base(name, returnTypeName)
        {
            Arguments = arguments;
            Prototype = prototype;
        }

        public System.Collections.Generic.IEnumerable<Emit.ArgumentType> ArgumentTypes()
        {
            foreach (var arg in Arguments)
            {
                yield return new Emit.ArgumentType(arg.Name, arg.TypeName);
            }
        }

        public System.Collections.Generic.IEnumerable<System.Type> RuntimeTypes()
        {
            return Arguments.Select(arg => Emit.TypeUtils.GetType(arg.TypeName));
        }

        public RuntimeType ReturnType()
        {
            return Emit.TypeUtils.GetPrimitiveType(TypeName);
        }

        public override string ToString()
        {
            return string.Concat("(", string.Join(",", Arguments.Select(arg => arg.ToString())), "):", TypeName.ToString());
        }


    }
}
