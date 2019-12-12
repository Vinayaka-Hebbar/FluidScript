using FluidScript.Reflection.Emit;

namespace FluidScript.Compiler.SyntaxTree
{
    public sealed class ThisExpression : Expression
    {
        public ThisExpression() : base(ExpressionType.This)
        {
        }

        public override TResult Accept<TResult>(IExpressionVisitor<TResult> visitor)
        {
            return visitor.VisitThis(this);
        }

        public override void GenerateCode(MethodBodyGenerator generator)
        {
            Type = generator.TypeGenerator;
            generator.LoadArgument(0);
        }

        public override string ToString()
        {
            return "this";
        }
    }
}
