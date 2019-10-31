using System.Collections.Generic;

namespace FluidScript.Compiler.SyntaxTree
{
    public class TypeDefinitionStatement : Statement
    {
        public readonly TypeDeclaration Declaration;
        public readonly Scopes.ObjectScope Scope;

        public Reflection.DeclaredMember DeclaredType;

        public readonly IList<Statement> Statements;
        public TypeDefinitionStatement(TypeDeclaration declaration, Statement[] statements, Reflection.DeclaredMember type) : base(StatementType.Class)
        {
            Declaration = declaration;
            Scope = declaration.Scope;
            Statements = new List<Statement>(statements);
            DeclaredType = type;

        }

        public System.Type Create(string assemblyName)
        {
            return Declaration.Create(assemblyName);
        }
    }
}
