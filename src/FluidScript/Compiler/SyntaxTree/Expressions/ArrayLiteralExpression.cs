using FluidScript.Reflection.Emit;
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
        public readonly Expression[] Expressions;

        /// <summary>
        /// Array type
        /// </summary>
        public TypeSyntax ArrayType { get; }

        /// <summary>
        /// Initializes new <see cref="ArrayLiteralExpression"/>
        /// </summary>
        public ArrayLiteralExpression(Expression[] expressions, TypeSyntax type) : base(ExpressionType.Array)
        {
            Expressions = expressions;
            ArrayType = type;
        }

        /// <inheritdoc/>
        public override TResult Accept<TResult>(IExpressionVisitor<TResult> visitor)
        {
            return visitor.VisitArrayLiteral(this);
        }

        /// <inheritdoc/>
        public override void GenerateCode(MethodBodyGenerator generator)
        {
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
