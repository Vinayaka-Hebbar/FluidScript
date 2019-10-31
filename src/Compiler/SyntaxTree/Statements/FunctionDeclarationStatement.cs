namespace FluidScript.Compiler.SyntaxTree
{
    public class FunctionDeclarationStatement : Statement
    {
        public readonly string Name;

        public readonly ArgumentInfo[] Arguments;

        public readonly string ReturnTypeName;

        public FunctionDeclarationStatement(FunctionDeclaration declaration) : base(StatementType.Declaration)
        {
            Name = declaration.Name;
            ReturnTypeName = declaration.ReturnTypeName;
            Arguments = declaration.Arguments;
        }

        protected FunctionDeclarationStatement(FunctionDeclaration declaration, StatementType nodeType) : base(nodeType)
        {
            Name = declaration.Name;
            ReturnTypeName = declaration.ReturnTypeName;
            Arguments = declaration.Arguments;
        }
    }
}
