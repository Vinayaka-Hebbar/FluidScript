namespace FluidScript.Compiler.SyntaxTree
{
    public class VariableDeclarationStatement : Statement
    {
        public readonly VariableDeclarationExpression[] DeclarationExpressions;
        public VariableDeclarationStatement(VariableDeclarationExpression[] declarationExpressions) : base(NodeType.Declaration)
        {
            DeclarationExpressions = declarationExpressions;
        }

        public override TReturn Accept<TReturn>(INodeVisitor<TReturn> visitor)
        {
            return visitor.VisitVarDefination(this);
        }
    }
}
