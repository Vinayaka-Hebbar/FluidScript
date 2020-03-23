using FluidScript.Compiler.Emit;

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
            var conventions = generator.Method.CallingConvention;
            if ((conventions & System.Reflection.CallingConventions.HasThis) == System.Reflection.CallingConventions.HasThis)
            {
                generator.LoadArgument(0);
            }
            else
            {
                // for annonymous type
                var variable = generator.GetLocalVariable("__value");
                generator.LoadVariable(variable);
            }
        }

        public override string ToString()
        {
            return "this";
        }
    }
}
