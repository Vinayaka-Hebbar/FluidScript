using System;

namespace FluidScript.Compiler.SyntaxTree
{
    internal class CustomExpression : Expression
    {
        readonly Action<Emit.MethodBodyGenerator> CustomGeneration;

        public CustomExpression(Action<Emit.MethodBodyGenerator> custom) : base(ExpressionType.Custom)
        {
            CustomGeneration = custom;
        }

        public override void GenerateCode(Emit.MethodBodyGenerator generator, Emit.MethodGenerateOption option)
        {
            CustomGeneration(generator);
        }
    }
}
