using FluidScript.Compiler.Binders;
using FluidScript.Compiler.Emit;
using System.Linq;

namespace FluidScript.Compiler.SyntaxTree
{
    public sealed class InvocationExpression : Expression
    {
        public readonly Expression Target;

        public readonly INodeList<Expression> Arguments;

        public System.Reflection.MethodInfo Method { get; set; }

        /// <summary>
        /// Argument convert list
        /// </summary>
        public ArgumentConversions Convertions { get; set; }

        public InvocationExpression(Expression target, NodeList<Expression> arguments) : base(ExpressionType.Invocation)
        {
            Target = target;
            Arguments = arguments;
        }


        public override TResult Accept<TResult>(IExpressionVisitor<TResult> visitor)
        {
            return visitor.VisitCall(this);
        }

        public override void GenerateCode(MethodBodyGenerator generator, MethodGenerateOption option)
        {
            Target.GenerateCode(generator);
            if (Arguments.Count > 0)
            {
                var conversions = Convertions;
                for (int i = 0; i < Arguments.Count; i++)
                {
                    var arg = Arguments[i];
                    var item = conversions[i];
                    if (item != null)
                    {
                        if (item.ConversionType == ConversionType.Convert)
                        {
                            arg.GenerateCode(generator);
                            item.GenerateCode(generator);
                        }
                        else if (item.ConversionType == ConversionType.ParamArray)
                        {
                            var arguments = new Expression[Arguments.Count - i];
                            Arguments.CopyTo(arguments, item.Index);
                            item.GenerateCode(generator, arguments);
                            break;
                        }
                    }
                    else
                    {
                        arg.GenerateCode(generator);
                    }
                }
            }
            // remaing binding
            if (Arguments.Count < Convertions.Count)
            {
                for (var i = Arguments.Count; i < Convertions.Count; i++)
                {
                    Convertions[i].GenerateCode(generator);
                }
            }
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
