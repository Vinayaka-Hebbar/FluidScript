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
            Expression expression = Expression.Accept(generator);
            expression.GenerateCode(generator);
                
        }

        public static explicit operator ExpressionStatement(Expression expression)
        {
            return new ExpressionStatement(expression);
        }

        public override string ToString()
        {
            return Expression.ToString();
        }
    }
}
