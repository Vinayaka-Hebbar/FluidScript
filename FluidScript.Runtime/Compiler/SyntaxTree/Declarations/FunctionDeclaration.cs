using System.Linq;

namespace FluidScript.Compiler.SyntaxTree
{
    public class FunctionDeclaration : MemberDeclaration
    {
        public readonly string Name;
        public readonly ArgumentInfo[] Arguments;
        public readonly BlockStatement Body;
        public readonly Emit.TypeName ReturnType;
        //todo modifiers

        public FunctionDeclaration(string name, ArgumentInfo[] arguments, Emit.TypeName returnType, BlockStatement body)
        {
            Name = name;
            Arguments = arguments;
            ReturnType = returnType;
            Body = body;
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

        public RuntimeType GetReturnType()
        {
            return Emit.TypeUtils.GetPrimitiveType(ReturnType);
        }

        public override string ToString()
        {
            return string.Concat("(", string.Join(",", Arguments.Select(arg => arg.ToString())), "):", ReturnType.ToString());
        }


    }
}
