using FluidScript.Reflection.Emit;
using System.Linq;

namespace FluidScript.Compiler.SyntaxTree
{
    public sealed class LocalDeclarationStatement : Statement
    {
        public readonly VariableDeclarationExpression[] DeclarationExpressions;
        public readonly bool IsReadOnly;

        public LocalDeclarationStatement(VariableDeclarationExpression[] declarationExpressions, bool isReadOnly) : base(StatementType.Declaration)
        {
            DeclarationExpressions = declarationExpressions;
            IsReadOnly = isReadOnly;
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
