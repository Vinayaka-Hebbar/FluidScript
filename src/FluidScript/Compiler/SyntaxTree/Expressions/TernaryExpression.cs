using FluidScript.Compiler.Emit;

namespace FluidScript.Compiler.SyntaxTree
{
    public sealed class TernaryExpression : Expression
    {
        public readonly Expression First;

        public readonly Expression Second;

        public readonly Expression Third;

        public System.Reflection.MethodInfo ImplicitCall { get; internal set; }

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

        public override void GenerateCode(MethodBodyGenerator generator, MethodGenerateOption options)
        {
            // Find the result type.
            var resultType = Type;
            var firstType = Second.Type;
            var secondType = Third.Type;
            First.GenerateCode(generator);
            // Branch if the condition is false.
            var startOfElse = generator.CreateLabel();
            generator.BranchIfFalse(startOfElse);
            Second.GenerateCode(generator);
            if (resultType != firstType)
            {
                generator.Call(ImplicitCall);
            }
            // Branch to the end.
            var end = generator.CreateLabel();
            generator.Branch(end);
            generator.DefineLabelPosition(startOfElse);

            Third.GenerateCode(generator);
            if (resultType != secondType)
            {
                generator.Call(ImplicitCall);
            }
            generator.DefineLabelPosition(end);
        }

        public override string ToString()
        {
            return string.Concat(First, '?', Second, ':', Third);
        }
    }
}
