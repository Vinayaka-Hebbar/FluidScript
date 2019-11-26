using System.Linq;
using System.Runtime.InteropServices;

namespace FluidScript.Compiler.SyntaxTree
{
    public class AnonymousFunctionExpression : Expression
    {
        public AnonymousFunctionExpression(ArgumentInfo[] arguments, BlockStatement body) : base(ExpressionType.Function)
        {
            Arguments = arguments;
            Body = body;
        }

        public ArgumentInfo[] Arguments { get; }

        public BlockStatement Body { get; }

#if Runtime
        public override RuntimeObject Evaluate([Optional] RuntimeObject instance)
        {
            var arguments = ArgumentTypes().ToArray();
            return new Core.AnonymousFunction(instance, this, arguments, RuntimeType.Any, DynamicInvoke);
        }
#endif

        internal RuntimeObject DynamicInvoke(RuntimeObject obj, RuntimeObject[] args)
        {
            var prototype = new Metadata.FunctionPrototype(obj.GetPrototype());
            var instance = new Core.LocalInstance(prototype, obj);
            var arguments = ArgumentTypes().ToArray();
            for (int index = 0; index < arguments.Length; index++)
            {
                var arg = arguments[index];
                instance[arg.Name] = arg.IsVarArgs() ? new Library.ArrayObject(args.Skip(index).ToArray(), arg.RuntimeType) : args[index];
            }
            return Body.Evaluate(instance);
        }

        public System.Collections.Generic.IEnumerable<Emit.ArgumentType> ArgumentTypes()
        {
            foreach (var arg in Arguments)
            {
                yield return new Emit.ArgumentType(arg.Name, arg.TypeName);
            }
        }

        public override string ToString()
        {
            return string.Concat("(", string.Join(",", Arguments.Select(arg => arg.ToString())), "):", TypeName.ToString());
        }
    }
}
