using FluidScript.Reflection.Emit;
using System.Linq;

namespace FluidScript.Compiler.SyntaxTree
{
    public sealed class InvocationExpression : Expression
    {
        public readonly Expression Target;
        public readonly Expression[] Arguments;

        public System.Reflection.MethodInfo Method { get; internal set; }

        public InvocationExpression(Expression target, Expression[] arguments) : base(ExpressionType.Invocation)
        {
            Target = target;
            Arguments = arguments;
        }


        public override TResult Accept<TResult>(IExpressionVisitor<TResult> visitor)
        {
            return visitor.VisitCall(this);
        }

        public override void GenerateCode(MethodBodyGenerator generator)
        {
            foreach (var item in Arguments)
            {
                item.GenerateCode(generator);
            }
            if (Target.NodeType == ExpressionType.Identifier && Method.IsStatic == false)
            {
                generator.LoadArgument(0);
            }
            Target.GenerateCode(generator);
            generator.Call(Method);
        }

        internal static void GenerateCall(MethodBodyGenerator generator, System.Reflection.MethodBase method, System.Collections.Generic.IEnumerable<Expression> arguments)
        {
            foreach (var item in arguments)
            {
                item.GenerateCode(generator);
            }
            generator.Call(method);
        }

        public override string ToString()
        {
            return string.Concat(Target.ToString(), "(", string.Join(",", Arguments.Select(arg => arg.ToString())), ")");
        }
    }
}
