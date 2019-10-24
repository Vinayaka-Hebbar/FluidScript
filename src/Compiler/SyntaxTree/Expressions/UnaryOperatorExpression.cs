namespace FluidScript.Compiler.SyntaxTree
{
    public class UnaryOperatorExpression : Expression
    {
        public readonly Expression Operand;

        public UnaryOperatorExpression(Expression operand, NodeType opcode)
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
