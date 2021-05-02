using System;

namespace FluidScript.Compiler.SyntaxTree
{
    internal class CustomExpression : Expression
    {
        readonly Action<Expression, Emit.MethodBodyGenerator> CustomGeneration;

        public CustomExpression(Action<Expression, Emit.MethodBodyGenerator> custom) : base(ExpressionType.Custom)
        {
            CustomGeneration = custom;
        }

        public override void GenerateCode(Emit.MethodBodyGenerator generator, Emit.MethodCompileOption option)
        {
            CustomGeneration(this, generator);
        }
    }
}
