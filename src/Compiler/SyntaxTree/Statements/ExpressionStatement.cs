namespace FluidScript.Compiler.SyntaxTree
{
    public class ExpressionStatement : Statement
    {
        public readonly Expression Expression;

        public ExpressionStatement(Expression expression, StatementType nodeType) : base(nodeType)
        {
            Expression = expression;
        }
    }
}
