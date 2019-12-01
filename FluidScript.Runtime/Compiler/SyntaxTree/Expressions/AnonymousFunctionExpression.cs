using System.Linq;

namespace FluidScript.Compiler.SyntaxTree
{
    public class AnonymousFunctionExpression : Expression
    {
        public readonly TypeParameter[] Parameters;

        public readonly TypeSyntax ReturnType;

        public readonly BlockStatement Body;

        public AnonymousFunctionExpression(TypeParameter[] parameters, TypeSyntax returnType, BlockStatement body) : base(ExpressionType.Function)
        {
            Parameters = parameters;
            ReturnType = returnType;
            Body = body;
        }


#if Runtime
        public override RuntimeObject Evaluate(RuntimeObject instance)
        {
            return new Core.AnonymousFunction(instance, this, Parameters.Select(para => para.GetParameterInfo()).ToArray(), ReturnType.GetTypeInfo(), this.DynamicInvoke);
        }
#endif

        internal RuntimeObject DynamicInvoke(RuntimeObject obj, RuntimeObject[] args)
        {
            var prototype = new Metadata.FunctionPrototype(obj.GetPrototype());
            var instance = new Core.LocalInstance(prototype, obj);
            var arguments = Parameters.Select(para => para.GetParameterInfo()).ToArray();
            for (int index = 0; index < arguments.Length; index++)
            {
                var arg = arguments[index];
                instance[arg.Name] = arg.IsVar ? new Library.ArrayObject(args.Skip(index).ToArray(), arg.Type.RuntimeType) : args[index];
            }
            return Body.Evaluate(instance);
        }

        public override string ToString()
        {
            //todo return type
            return string.Concat("(", string.Join(",", Parameters.Select(arg => arg.ToString())), "):any");
        }
    }
}
