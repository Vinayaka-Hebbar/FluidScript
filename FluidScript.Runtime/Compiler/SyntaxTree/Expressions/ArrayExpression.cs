namespace FluidScript.Compiler.SyntaxTree
{
    public class ArrayExpression : Expression
    {
        public readonly Expression[] Expressions;
        public ArrayExpression(Expression[] expressions) : base(ExpressionType.Block)
        {
            Expressions = expressions;
        }
    }
}
