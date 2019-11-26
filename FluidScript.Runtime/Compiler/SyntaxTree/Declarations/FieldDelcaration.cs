namespace FluidScript.Compiler.SyntaxTree
{
    public sealed class FieldDelcaration : MemberDeclaration
    {
        public readonly VariableDeclarationExpression[] VariableDeclarations;

        public FieldDelcaration(VariableDeclarationExpression[] variableDeclarations)
        {
            VariableDeclarations = variableDeclarations;
        }
    }
}
