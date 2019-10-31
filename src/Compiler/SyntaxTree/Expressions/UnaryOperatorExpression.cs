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

        public override PrimitiveType ResultType
        {
            get
            {
                return Operand.ResultType;
            }
        }

        public override void GenerateCode(ILGenerator generator, OptimizationInfo info)
        {
            if (NodeType == ExpressionType.Parenthesized)
                Operand.GenerateCode(generator, info);
            ResolvedType = Operand.Type;
        }
    }
}
