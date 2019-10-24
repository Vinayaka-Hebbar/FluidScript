namespace FluidScript.Compiler.SyntaxTree
{
    public class ExpressionStatement : Statement
    {
        public readonly Expression Expression;

        public ExpressionStatement(Expression expression, NodeType opCode) : base(opCode)
        {
            Expression = expression;
        }

        public override TReturn Accept<TReturn>(INodeVisitor<TReturn> visitor)
        {
            return Expression.Accept(visitor);
        }
    }
}
