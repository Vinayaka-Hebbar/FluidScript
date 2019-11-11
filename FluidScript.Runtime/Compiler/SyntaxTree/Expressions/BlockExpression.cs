namespace FluidScript.Compiler.SyntaxTree
{
    public class BlockExpression : Expression
    {
        public readonly Expression[] expressions;
        public BlockExpression(Expression[] expressions) : base(ExpressionType.Block)
        {
            this.expressions = expressions;
        }

        public override RuntimeObject Evaluate()
        {
            return base.Evaluate();
        }
    }
}
