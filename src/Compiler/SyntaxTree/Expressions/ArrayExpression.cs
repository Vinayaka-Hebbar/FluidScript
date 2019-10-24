namespace FluidScript.Compiler.SyntaxTree
{
    public class ArrayExpression : Expression
    {
        public readonly Expression[] Expressions;
        public ArrayExpression(Expression[] expressions) : base(NodeType.Block)
        {
            Expressions = expressions;
        }

        public override TReturn Accept<TReturn>(INodeVisitor<TReturn> visitor)
        {
            return visitor.VisitExpressions(this);
        }
    }
}
