namespace FluidScript.Compiler.SyntaxTree
{
    public class FunctionDeclarationStatement : Statement
    {
        public readonly string Name;

        public readonly FunctionDeclaration Declaration;

        public FunctionDeclarationStatement(FunctionDeclaration declaration) : base(StatementType.Declaration)
        {
            Name = declaration.Name;
            Declaration = declaration;
        }

        protected FunctionDeclarationStatement(FunctionDeclaration declaration, StatementType nodeType) : base(nodeType)
        {
            Name = declaration.Name;
            Declaration = declaration;
        }

        public virtual System.Reflection.MethodInfo Create()
        {
            throw new System.Exception("Method not declared");
        }

        public override string ToString()
        {
            return Declaration.ToString();
        }
    }
}
