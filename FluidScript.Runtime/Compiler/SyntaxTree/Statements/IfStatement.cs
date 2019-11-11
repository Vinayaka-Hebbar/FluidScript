namespace FluidScript.Compiler.SyntaxTree
{
    public sealed class IfStatement : Statement
    {
        public readonly Expression Expression;
        public readonly Statement Then;
        public readonly Statement Other;

        public IfStatement(Expression expression, Statement then, Statement other) : base(StatementType.If)
        {
            Expression = expression;
            Then = then;
            Other = other;
        }

        public override RuntimeObject Evaluate()
        {
            if (Expression.Evaluate().ToBool())
                return Then.Evaluate();
            return Other != null ? Other.Evaluate() : RuntimeObject.Null;
        }


    }
}
