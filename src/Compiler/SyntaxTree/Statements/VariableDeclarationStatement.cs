using FluidScript.Compiler.Emit;

namespace FluidScript.Compiler.SyntaxTree
{
    public class VariableDeclarationStatement : Statement
    {
        public readonly VariableDeclarationExpression[] DeclarationExpressions;
        public VariableDeclarationStatement(VariableDeclarationExpression[] declarationExpressions) : base(StatementType.Declaration)
        {
            DeclarationExpressions = declarationExpressions;
        }

        public override void GenerateCode(ILGenerator generator, OptimizationInfo info)
        {
            foreach (var declaration in DeclarationExpressions)
            {
                declaration.GenerateCode(generator, info);
            }
        }

        public override TReturn Accept<TReturn>(INodeVisitor<TReturn> visitor)
        {
            return visitor.VisitVarDefination(this);
        }
    }
}
