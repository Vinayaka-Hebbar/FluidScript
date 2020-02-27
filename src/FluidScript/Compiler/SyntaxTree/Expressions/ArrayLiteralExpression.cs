using FluidScript.Compiler.Emit;
using System;
using System.Linq;

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
        public readonly NodeList<Expression> Expressions;

        /// <summary>
        /// Array Size
        /// </summary>
        public readonly Expression Size;


        /// <summary>
        /// Array type
        /// </summary>
        public readonly TypeSyntax ArrayType;

        /// <summary>
        /// Initializes new <see cref="ArrayLiteralExpression"/>
        /// </summary>
        public ArrayLiteralExpression(NodeList<Expression> expressions, TypeSyntax type, Expression size) : base(ExpressionType.Array)
        {
            Expressions = expressions;
            ArrayType = type;
            Size = size;
        }

        /// <inheritdoc/>
        public override TResult Accept<TResult>(IExpressionVisitor<TResult> visitor)
        {
            return visitor.VisitArrayLiteral(this);
        }

        /// <inheritdoc/>
        public override void GenerateCode(MethodBodyGenerator generator)
        {
            if (Size != null)
            {
                Size.GenerateCode(generator);
                generator.CallStatic(Utils.ReflectionHelpers.Integer_to_Int32);
            }
            else
                generator.LoadInt32(Expressions.Length);
            Type type = Type.GetElementType();
            generator.NewArray(type);
            for (int i = 0; i < Expressions.Length; i++)
            {
                generator.Duplicate();
                generator.LoadInt32(i);
                var expression = Expressions[i];
                if (expression == null)
                    generator.LoadNull();
                else
                {
                    expression.GenerateCode(generator);
                    //todo box
                    //EmitConvertion.ToAny(generator, type);
                }
                generator.StoreArrayElement(type);
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Concat("[", string.Join(",", Expressions.Select(exp => exp.ToString())), "]");
        }
    }
}
