namespace FluidScript.SyntaxTree.Statements
{
    public class ExpressionStatement : Statement
    {
        public readonly IExpression Expression;

        public ExpressionStatement(IExpression expression, Operation opCode) : base(opCode)
        {
            Expression = expression;
        }

        public override TReturn Accept<TReturn>(INodeVisitor<TReturn> visitor)
        {
            return Expression.Accept(visitor);
        }
    }
}
