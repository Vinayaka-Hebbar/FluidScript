﻿using FluidScript.Compiler.Emit;
using FluidScript.Runtime;
using System.Linq;

namespace FluidScript.Compiler.SyntaxTree
{
    public sealed class InvocationExpression : Expression
    {
        public readonly Expression Target;

        public readonly INodeList<Expression> Arguments;

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

        public override void GenerateCode(MethodBodyGenerator generator, MethodGenerateOption option)
        {
            Target.GenerateCode(generator);
            EmitArguments(generator, Arguments, Conversions);
            generator.Call(Method);
            // if current value must not be returned for assigment
            if ((option & MethodGenerateOption.Return) == 0 && Type is object && Type != TypeProvider.VoidType)
                generator.Pop();
        }

        internal static void EmitArguments(MethodBodyGenerator generator, INodeList<Expression> arguments, ArgumentConversions conversions)
        {
            if (arguments.Count > 0)
            {
                for (int i = 0; i < arguments.Count; i++)
                {
                    var arg = arguments[i];
                    var conv = conversions[i];
                    if (conv != null)
                    {
                        if (conv.ConversionType == ConversionType.Normal)
                        {
                            arg.GenerateCode(generator, AssignOption);
                            generator.EmitConvert(conv);
                        }
                        else if (conv.ConversionType == ConversionType.ParamArray)
                        {
                            var args = new Expression[arguments.Count - i];
                            arguments.CopyTo(args, conv.Index);
                            generator.EmitConvert((ParamArrayConversion)conv, args);
                            break;
                        }
                    }
                    else
                    {
                        arg.GenerateCode(generator, AssignOption);
                    }
                }
            }
            // remaing Conversions like optional or param array
            if (arguments.Count < conversions.Count)
            {
                for (var i = arguments.Count; i < conversions.Count; i++)
                {
                    Conversion conv = conversions[i];
                    if (conv.ConversionType == ConversionType.Normal)
                    {
                        arguments[i].GenerateCode(generator);
                        generator.EmitConvert(conv);
                    }
                    else if (conv.ConversionType == ConversionType.ParamArray)
                    {
                        // remaining arguments
                        var args = new Expression[arguments.Count - i];
                        arguments.CopyTo(args, conv.Index);
                        generator.EmitConvert((ParamArrayConversion)conv, args);
                        break;
                    }
                }
            }
        }

        public override string ToString()
        {
            return string.Concat(Target.ToString(), "(", string.Join(",", Arguments.Select(arg => arg.ToString())), ")");
        }
    }
}
