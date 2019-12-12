using FluidScript.Reflection.Emit;

namespace FluidScript.Compiler.SyntaxTree
{
    public sealed class NullExpression : Expression
    {
        public NullExpression() : base(ExpressionType.Literal)
        {
        }

        public override void GenerateCode(MethodBodyGenerator generator)
        {
            generator.LoadNull();
        }
    }
}
