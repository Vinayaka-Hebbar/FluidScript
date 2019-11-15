namespace FluidScript.Compiler.SyntaxTree
{
    public class ExpressionStatement : Statement
    {
        public readonly Expression Expression;

        public ExpressionStatement(Expression expression) : base(StatementType.Expression)
        {
            Expression = expression;
        }

#if Runtime
        public override RuntimeObject Evaluate(RuntimeObject instance)
        {
            return Expression.Evaluate(instance);
        }
#endif

        public override void GenerateCode(Emit.ILGenerator generator, Emit.MethodOptimizationInfo info)
        {
            Expression.GenerateCode(generator, info);
        }

        public override string ToString()
        {
            return Expression.ToString();
        }
    }
}
