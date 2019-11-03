namespace FluidScript.Compiler.SyntaxTree
{
    public class FunctionDeclarationStatement : Statement
    {
        public readonly string Name;

        public readonly ArgumentInfo[] Arguments;

        public readonly Emit.TypeName ReturnTypeName;

        public FunctionDeclarationStatement(FunctionDeclaration declaration) : base(StatementType.Declaration)
        {
            Name = declaration.Name;
            ReturnTypeName = declaration.TypeName;
            Arguments = declaration.Arguments;
        }

        protected FunctionDeclarationStatement(FunctionDeclaration declaration, StatementType nodeType) : base(nodeType)
        {
            Name = declaration.Name;
            ReturnTypeName = declaration.TypeName;
            Arguments = declaration.Arguments;
        }
    }
}
