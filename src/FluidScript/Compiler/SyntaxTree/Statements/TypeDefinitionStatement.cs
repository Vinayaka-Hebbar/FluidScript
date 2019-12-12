using System.Collections.Generic;

namespace FluidScript.Compiler.SyntaxTree
{
#if Type
    public class TypeDefinitionStatement : Statement
    {
        public readonly TypeDeclaration Declaration;
        public readonly Scopes.ObjectScope Scope;

        public Reflection.DeclaredMember DeclaredType;

        public readonly IList<Node> Nodes;
        public TypeDefinitionStatement(TypeDeclaration declaration, Node[] nodes, Reflection.DeclaredMember type) : base(StatementType.Class)
        {
            Declaration = declaration;
            Scope = declaration.Scope;
            Nodes = new List<Node>(nodes);
            DeclaredType = type;
        }

        public override IEnumerable<Node> ChildNodes => Nodes;

        public System.Type Create(string assemblyName)
        {
            return Declaration.Create(assemblyName);
        }
    }
#endif
}
