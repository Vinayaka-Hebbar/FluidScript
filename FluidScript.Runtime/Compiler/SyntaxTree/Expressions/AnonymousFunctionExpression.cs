using System.Linq;
using System.Runtime.InteropServices;

namespace FluidScript.Compiler.SyntaxTree
{
    public class AnonymousFunctionExpression : Expression
    {
        public readonly Metadata.FunctionPrototype Prototype;

        public AnonymousFunctionExpression(ArgumentInfo[] arguments, BodyStatement body, Metadata.FunctionPrototype prototype) : base(ExpressionType.Function)
        {
            Arguments = arguments;
            Body = body;
            Prototype = prototype;
        }

        public ArgumentInfo[] Arguments { get; }

        public BodyStatement Body { get; }

        public System.Collections.Generic.IEnumerable<Emit.ArgumentType> ArgumentTypes()
        {
            foreach (var arg in Arguments)
            {
                yield return new Emit.ArgumentType(arg.Name, arg.TypeName.GetRuntimeType(), arg.TypeName.Flags);
            }
        }

#if Runtime
        public override RuntimeObject Evaluate([Optional] RuntimeObject instance)
        {
            var declaredMethod = new Reflection.DeclaredMethod("Anonymous", Arguments.Select(arg => new Emit.ArgumentType(arg.Name, arg.TypeName)).ToArray(), RuntimeType.Any)
            {
                Prototype = Prototype,
                ValueAtTop = Body
            };
            return new Metadata.DynamicFunction(declaredMethod, instance, declaredMethod.DynamicInvoke);
        }
#endif

        public override string ToString()
        {
            return string.Concat("(", string.Join(",", Arguments.Select(arg => arg.ToString())), "):", TypeName.ToString());
        }
    }
}
