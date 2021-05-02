using FluidScript.Compiler.Emit;
using FluidScript.Runtime;
using System.Linq;

namespace FluidScript.Compiler.SyntaxTree
{
    public sealed class InvocationExpression : Expression
    {
        public readonly Expression Target;

        public readonly NodeList<Expression> Arguments;

        public System.Reflection.MethodBase Method { get; set; }

        /// <summary>
        /// Argument convert list
        /// </summary>
        public ArgumentConversions Conversions { get; set; }

        public InvocationExpression(Expression target, NodeList<Expression> arguments) : base(ExpressionType.Invocation)
        {
            Target = target;
            Arguments = arguments;
        }

        public override TResult Accept<TResult>(IExpressionVisitor<TResult> visitor)
        {
            return visitor.VisitCall(this);
        }

        public override void GenerateCode(MethodBodyGenerator generator, MethodCompileOption option)
        {
            if (Method.IsAbstract && Method.DeclaringType == typeof(IDynamicInvocable))
            {
                Target.GenerateCode(generator);
                if (Target.Type.IsValueType)
                    generator.Box(Target.Type);
            }
            else
            {
                Target.GenerateCode(generator, MethodCompileOption.EmitStartAddress);
            }
            if (Target.NodeType == ExpressionType.MemberAccess)
            {
                MemberExpression target = (MemberExpression)Target;
                if (target.Target.Type.IsValueType)
                {
                    switch (target.Target.NodeType)
                    {
                        case ExpressionType.Indexer:
                        case ExpressionType.MemberAccess:
                            var temp = generator.DeclareVariable(target.Target.Type);
                            generator.StoreVariable(temp);
                            generator.LoadAddressOfVariable(temp);
                            break;
                    }
                }
            }
            else if (Target.NodeType == ExpressionType.Identifier)
            {
                // if it an identifier expression this might be local member call
                var exp = (NameExpression)Target;
                if (exp.Binder == null && Method.IsStatic == false)
                    generator.LoadArgument(0);
            }
            generator.EmitArguments(Arguments, Conversions);
            generator.Call(Method);
            // if current value must not be returned for assigment
            if ((option & MethodCompileOption.Return) == 0 && Type is object && Type != TypeProvider.VoidType)
                generator.Pop();
        }

        public override string ToString()
        {
            return string.Concat(Target.ToString(), "(", string.Join(",", Arguments.Select(arg => arg.ToString())), ")");
        }
    }
}
