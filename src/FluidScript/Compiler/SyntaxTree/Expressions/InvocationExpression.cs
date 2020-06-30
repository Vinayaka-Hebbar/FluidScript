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
                    var conv = conversions[i];
                    if (conv != null)
                    {
                        if (conv.ConversionType == ConversionType.Normal)
                        {
                            arg.GenerateCode(generator);
                            conv.GenerateCode(generator);
                        }
                        else if (conv.ConversionType == ConversionType.ParamArray)
                        {
                            var arguments = new Expression[Arguments.Count - i];
                            Arguments.CopyTo(arguments, conv.Index);
                            conv.GenerateCode(generator, arguments);
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
                    Conversion conv = Convertions[i];
                    if (conv.ConversionType == ConversionType.Normal)
                    {
                        Arguments[i].GenerateCode(generator);
                        conv.GenerateCode(generator);
                    }
                    else if (conv.ConversionType == ConversionType.ParamArray)
                    {
                        // remaining arguments
                        var arguments = new Expression[Arguments.Count - i];
                        Arguments.CopyTo(arguments, conv.Index);
                        conv.GenerateCode(generator, arguments);
                        break;
                    }
                }
            }
            generator.Call(Method);
            if((option & MethodGenerateOption.Assign) == 0 && Type != TypeProvider.VoidType)
                generator.Pop();
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
