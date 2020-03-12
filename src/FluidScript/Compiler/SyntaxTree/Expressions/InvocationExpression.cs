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

        public override void GenerateCode(MethodBodyGenerator generator)
        {
            Target.GenerateCode(generator);
            for (int i = 0; i < Arguments.Length; i++)
            {
                var item = Arguments[i];
                var conversion = Convertions.At(i);
                if (conversion != null)
                {
                    if (conversion.ConversionType == ConversionType.Convert)
                    {
                        conversion.Generate(generator, item);
                    }
                    else if (conversion.ConversionType == ConversionType.ParamArray)
                    {
                        var arguments = new Expression[Arguments.Length - i];
                        Arguments.CopyTo(arguments, conversion.Index);
                        conversion.Generate(generator, arguments);
                        break;
                    }
                }
                else
                {
                    item.GenerateCode(generator);
                }
            }
            // remaing binding
            if (Arguments.Length < Convertions.Count)
            {
                foreach (var binder in Convertions.Skip(Arguments.Length))
                {
                    binder.Generate(generator);
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
