namespace FluidScript.Compiler.SyntaxTree
{
    public class ExpressionStatement : Statement
    {
        public readonly Expression Expression;

        public ExpressionStatement(Expression expression) : base(StatementType.Expression)
        {
            Expression = expression;
        }

#pragma warning disable CS0508 // 'ExpressionStatement.Evaluate()': return type must be 'RuntimeObject' to match overridden member 'Node.Evaluate()'
        public override object Evaluate()
#pragma warning restore CS0508 // 'ExpressionStatement.Evaluate()': return type must be 'RuntimeObject' to match overridden member 'Node.Evaluate()'
        {
            return Expression.Evaluate();
        }

        public override void GenerateCode(Emit.ILGenerator generator, Emit.MethodOptimizationInfo info)
        {
            Expression.GenerateCode(generator, info);
        }
    }
}
