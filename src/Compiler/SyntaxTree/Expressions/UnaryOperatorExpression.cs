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

        public override PrimitiveType PrimitiveType()
        {
            return Operand.PrimitiveType();
        }

        public override void GenerateCode(ILGenerator generator, MethodOptimizationInfo info)
        {
            if (NodeType == ExpressionType.Parenthesized)
                Operand.GenerateCode(generator, info);
            ResolvedType = Operand.ResultType();
        }
    }
}
