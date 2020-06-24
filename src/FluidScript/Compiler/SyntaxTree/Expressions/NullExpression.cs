using FluidScript.Compiler.Emit;

namespace FluidScript.Compiler.SyntaxTree
{
    /// <summary>
    /// Null Expression
    /// </summary>
    public sealed class NullExpression : Expression
    {
        public NullExpression() : base(ExpressionType.Literal)
        {
        }

        public override TResult Accept<TResult>(IExpressionVisitor<TResult> visitor)
        {
            return visitor.VisitNull(this);
        }

        public override void GenerateCode(MethodBodyGenerator generator, MethodGenerateOption options)
        {
            generator.LoadNull();
        }

        public override string ToString()
        {
            return "null";
        }
    }
}
