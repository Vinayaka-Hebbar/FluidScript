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

#if Runtime
        public override RuntimeObject Evaluate(RuntimeObject instance)
        {
            if (Expression.Evaluate(instance).ToBool())
                return Then.Evaluate(instance);
            return Other?.Evaluate(instance);
        }
#endif

    }
}
