namespace FluidScript.Compiler.SyntaxTree
{
    public class ExpressionStatement : Statement
    {
        public readonly Expression Expression;

        public ExpressionStatement(Expression expression) : base(StatementType.Expression)
        {
            Expression = expression;
        }

        protected internal override void Accept(IStatementVisitor visitor)
        {
            visitor.VisitExpression(this);
        }

        public override void GenerateCode(Emit.MethodBodyGenerator generator)
        {
            Expression.Accept(generator).GenerateCode(generator);
        }

        public override string ToString()
        {
            return Expression.ToString();
        }
    }
}
