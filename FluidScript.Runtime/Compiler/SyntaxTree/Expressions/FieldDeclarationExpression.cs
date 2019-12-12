namespace FluidScript.Compiler.SyntaxTree
{
    public sealed class FieldDeclarationExpression : DeclarationExpression
    {
        public readonly TypeSyntax FieldType;
        public readonly Expression Value;

        public FieldDeclarationExpression(string name, TypeSyntax type, Expression value) : base(name)
        {
            FieldType = type;
            Value = value;

        }
    }
}
