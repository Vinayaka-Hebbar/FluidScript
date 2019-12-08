using FluidScript.Reflection.Emit;
using System;
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

#if Runtime
        public override RuntimeObject Evaluate(RuntimeObject instance)
        {
            var args = new RuntimeObject[Arguments.Length];
            for (int i = 0; i < Arguments.Length; i++)
            {
                args[i] = Arguments[i].Evaluate(instance);
            }
            if (Target.NodeType == ExpressionType.Identifier)
            {
                var value = (NameExpression)Target;
                return instance.Call(value.Name, args);
            }
            else if (Target.NodeType == ExpressionType.MemberAccess)
            {
                var qualified = (MemberExpression)Target;
                var value = qualified.Target.Evaluate(instance);
                return value.Call(qualified.Name, args);
            }
            else
            {
                var value = Target.Evaluate(instance);
                return value.DynamicInvoke(args);
            }
        }

#endif

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
