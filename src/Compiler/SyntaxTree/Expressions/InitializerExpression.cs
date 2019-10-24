namespace FluidScript.Compiler.SyntaxTree
{
    public class InitializerExpression : VariableDeclarationExpression
    {
        public readonly Expression Target;

        public InitializerExpression(string id, Expression target) : base(id)
        {
            Target = target;
        }

        public override TReturn Accept<TReturn>(INodeVisitor<TReturn> visitor)
        {
            return visitor.VisitInitializer(this);
        }
    }
}
