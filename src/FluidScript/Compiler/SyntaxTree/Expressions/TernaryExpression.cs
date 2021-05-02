using FluidScript.Compiler.Emit;

namespace FluidScript.Compiler.SyntaxTree
{
    public sealed class TernaryExpression : Expression
    {
        public readonly Expression First;

        public readonly Expression Second;

        public readonly Expression Third;

        public Runtime.ArgumentConversions Conversions { get; set; }

        public System.Reflection.MethodInfo ExpressionConversion { get; set; }

        public TernaryExpression(Expression first, Expression second, Expression third) : base(ExpressionType.Question)
        {
            First = first;
            Second = second;
            Third = third;
        }

        public override TResult Accept<TResult>(IExpressionVisitor<TResult> visitor)
        {
            return visitor.VisitTernary(this);
        }

        public override void GenerateCode(MethodBodyGenerator generator, MethodCompileOption options)
        {
            // Find the result type.
            var resultType = Type;
            var firstType = Second.Type;
            var secondType = Third.Type;
            First.GenerateCode(generator, MethodCompileOption.Return);
            if (Conversions != null && Conversions.Count > 0)
            {
                generator.EmitConvert(Conversions[0]);
            }
            // Branch if the condition is false.
            var startOfElse = generator.CreateLabel();
            generator.BranchIfFalse(startOfElse);
            Second.GenerateCode(generator, options);
            if (resultType != firstType)
            {
                generator.Call(ExpressionConversion);
            }
            // Branch to the end.
            var end = generator.CreateLabel();
            generator.Branch(end);
            generator.DefineLabelPosition(startOfElse);

            Third.GenerateCode(generator, options);
            if (resultType != secondType)
            {
                generator.Call(ExpressionConversion);
            }
            generator.DefineLabelPosition(end);
        }

        public override string ToString()
        {
            return string.Concat(First, '?', Second, ':', Third);
        }
    }
}
