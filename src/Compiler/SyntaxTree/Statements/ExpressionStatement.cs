namespace FluidScript.Compiler.SyntaxTree
{
    public class ExpressionStatement : Statement
    {
        public readonly Expression Expression;

        public ExpressionStatement(Expression expression) : base(StatementType.Expression)
        {
            Expression = expression;
        }

        public override object GetValue()
        {
            return Expression.GetValue();
        }

        public override void GenerateCode(Emit.ILGenerator generator, Emit.MethodOptimizationInfo info)
        {
            Expression.GenerateCode(generator, info);
        }
    }
}
