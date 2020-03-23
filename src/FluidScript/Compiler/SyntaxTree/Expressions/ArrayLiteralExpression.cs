using FluidScript.Compiler.Emit;
using System;

namespace FluidScript.Compiler.SyntaxTree
{
    /// <summary>
    /// Array literal &lt;<see cref="ArrayType"/>&gt;[1,2]
    /// </summary>
    public sealed class ArrayLiteralExpression : Expression
    {
        /// <summary>
        /// List of array items
        /// </summary>
        public readonly INodeList<Expression> Expressions;

        /// <summary>
        /// Array Size
        /// </summary>
        public readonly INodeList<Expression> Arguments;


        /// <summary>
        /// Array type
        /// </summary>
        public readonly TypeSyntax ArrayType;

        /// <summary>
        /// Initializes new <see cref="ArrayLiteralExpression"/>
        /// </summary>
        public ArrayLiteralExpression(INodeList<Expression> expressions, TypeSyntax type, INodeList<Expression> arguments) : base(ExpressionType.Array)
        {
            Expressions = expressions;
            ArrayType = type;
            Arguments = arguments;
        }

        public System.Reflection.ConstructorInfo Constructor
        {
            get;
            set;
        }

        public Binders.ArgumentConversions ArgumentConversions { get; set; }

        public Binders.ArgumentConversions ArrayConversions { get; set; }

        /// <inheritdoc/>
        public override TResult Accept<TResult>(IExpressionVisitor<TResult> visitor)
        {
            return visitor.VisitArrayLiteral(this);
        }

        /// <inheritdoc/>
        public override void GenerateCode(MethodBodyGenerator generator)
        {
            if (Arguments != null)
            {
                var conversions = ArgumentConversions;
                for (int i = 0; i < Arguments.Length; i++)
                {
                    Arguments[i].GenerateCode(generator);
                    var conversion = conversions[i];
                    if (conversion != null)
                        conversion.GenerateCode(generator);
                }
            }
            generator.Call(Constructor);

            int length = Expressions.Length;
            if (length > 0)
            {
                var conversions = ArrayConversions;
                generator.LoadInt32(length);
                Type type = Type.GetElementType();
                generator.NewArray(type);
                for (int i = 0; i < Expressions.Length; i++)
                {
                    generator.Duplicate();
                    generator.LoadInt32(i);
                    var expression = Expressions[i];
                    expression.GenerateCode(generator);
                    var group = conversions[i];
                    if (group != null)
                        group.GenerateCode(generator);
                    generator.StoreArrayElement(type);
                }
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            if (ArrayType == null)
                return string.Concat("[", string.Join(",", Expressions.Map(exp => exp.ToString())), "]<any>(", string.Join(",", Arguments.Map(arg => arg.ToString())), ")");
            return string.Concat("[", string.Join(",", Expressions.Map(exp => exp.ToString())), "]<", ArrayType, ">(", string.Join(",", Arguments.Map(arg => arg.ToString())), ")");
        }
    }
}
