namespace FluidScript.Compiler.SyntaxTree
{
    public class VariableDeclarationExpression : Expression
    {
        public readonly string Id;

        public VariableDeclarationExpression(string id) : base(NodeType.Declaration)
        {
            Id = id;
        }

        public override TReturn Accept<TReturn>(INodeVisitor<TReturn> visitor)
        {
            return visitor.VisitVarDeclaration(this);
        }
    }
}
