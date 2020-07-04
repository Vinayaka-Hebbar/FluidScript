using FluidScript.Compiler.Emit;
using System.Linq;

namespace FluidScript.Compiler.SyntaxTree
{
    public sealed class LocalDeclarationStatement : Statement
    {
        public readonly NodeList<VariableDeclarationExpression> DeclarationExpressions;

        public LocalDeclarationStatement(NodeList<VariableDeclarationExpression> declarationExpressions) : base(StatementType.Declaration)
        {
            DeclarationExpressions = declarationExpressions;
        }


        protected internal override void Accept(IStatementVisitor visitor)
        {
            visitor.VisitDeclaration(this);
        }

        public override void GenerateCode(MethodBodyGenerator generator)
        {
            foreach (var declaration in DeclarationExpressions)
            {
                declaration.GenerateCode(generator, MethodCompileOption.None);
            }
        }

        public override string ToString()
        {
            return string.Concat("var ", string.Join(",", DeclarationExpressions.Select(e => e.ToString())));
        }
    }
}
