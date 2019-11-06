namespace FluidScript.Compiler.SyntaxTree
{
    public class FieldDeclarationExpression : Node
    {
        public readonly string Name;
        public readonly Reflection.DeclaredVariable Member;

        public FieldDeclarationExpression(string name, Reflection.DeclaredVariable member)
        {
            Name = name;
            Member = member;
        }
    }
}
