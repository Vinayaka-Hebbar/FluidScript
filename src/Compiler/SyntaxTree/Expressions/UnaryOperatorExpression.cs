using FluidScript.Compiler.Emit;

namespace FluidScript.Compiler.SyntaxTree
{
    public class UnaryOperatorExpression : Expression
    {
        public readonly Expression Operand;

        public UnaryOperatorExpression(Expression operand, ExpressionType opcode)
            : base(opcode)
        {
            Operand = operand;
        }

        protected override void ResolveType(OptimizationInfo info)
        {
            ResolvedType = Operand.ResultType(info);
            ResolvedPrimitiveType = Operand.PrimitiveType(info);
        }

        public override void GenerateCode(ILGenerator generator, MethodOptimizationInfo info)
        {
            if (NodeType == ExpressionType.Parenthesized)
                Operand.GenerateCode(generator, info);
        }
    }
}
