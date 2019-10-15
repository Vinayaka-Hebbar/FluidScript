namespace FluidScript.SyntaxTree.Expressions
{
    public class InitializerExpression : VariableDeclarationExpression
    {
        public readonly IExpression Target;

        public InitializerExpression(string id, IExpression target) : base(id)
        {
            Target = target;
        }

        public override TReturn Accept<TReturn>(INodeVisitor<TReturn> visitor)
        {
            return visitor.VisitInitializer(this);
        }
    }
}
