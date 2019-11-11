namespace FluidScript.Compiler.SyntaxTree
{
    public class FieldDeclarationStatement : Node
    {
        public readonly FieldDeclarationExpression[] Declarations;

        public FieldDeclarationStatement(FieldDeclarationExpression[] declarations)
        {
            Declarations = declarations;
        }

        public FieldDeclarationStatement(FieldDeclarationExpression declaration)
        {
            Declarations = new FieldDeclarationExpression[1] { declaration };
        }
    }
}
