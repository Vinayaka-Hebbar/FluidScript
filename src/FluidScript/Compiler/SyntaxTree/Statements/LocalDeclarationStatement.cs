using FluidScript.Reflection.Emit;
using System.Linq;

namespace FluidScript.Compiler.SyntaxTree
{
    public sealed class LocalDeclarationStatement : Statement
    {
        public readonly VariableDeclarationExpression[] DeclarationExpressions;

        public LocalDeclarationStatement(VariableDeclarationExpression[] declarationExpressions) : base(StatementType.Declaration)
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
                declaration.GenerateCode(generator);
            }
        }

        public override string ToString()
        {
            return string.Concat("var ", string.Join(",", DeclarationExpressions.Select(e => e.ToString())));
        }
    }
}
