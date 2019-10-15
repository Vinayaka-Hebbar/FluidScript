using FluidScript.SyntaxTree.Expressions;

namespace FluidScript.SyntaxTree.Statements
{
    public class VariableDeclarationStatement : Statement
    {
        public readonly VariableDeclarationExpression[] DeclarationExpressions;
        public VariableDeclarationStatement(VariableDeclarationExpression[] declarationExpressions) : base(Operation.Declaration)
        {
            DeclarationExpressions = declarationExpressions;
        }

        public override TReturn Accept<TReturn>(INodeVisitor<TReturn> visitor)
        {
            return visitor.VisitVarDefination(this);
        }
    }
}
