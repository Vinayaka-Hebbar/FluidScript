using FluidScript.Reflection.Emit;

namespace FluidScript.Compiler.SyntaxTree
{
    public class ThisExpression : Expression
    {
        public ThisExpression() : base(ExpressionType.This)
        {
        }

#if Runtime
        public override RuntimeObject Evaluate(RuntimeObject instance)
        {
            return instance["this"];
        }
#endif
        public override TResult Accept<TResult>(IExpressionVisitor<TResult> visitor)
        {
            return visitor.VisitThis(this);
        }

        public override void GenerateCode(MethodBodyGenerator generator)
        {
            ResolvedType = generator.TypeGenerator;
            generator.LoadArgument(0);
        }

        public override string ToString()
        {
            return "this";
        }
    }
}
