namespace FluidScript.SyntaxTree.Expressions
{
    public class UnaryOperatorExpression : Expression
    {
        public readonly IExpression Operand;

        public UnaryOperatorExpression(IExpression operand, Operation opcode)
            : base(opcode)
        {
            Operand = operand;
        }

        public override TReturn Accept<TReturn>(INodeVisitor<TReturn> visitor)
        {
            return visitor.VisitUnaryOperator(this);
        }
    }
}
