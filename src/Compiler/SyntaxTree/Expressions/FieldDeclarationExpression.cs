namespace FluidScript.Compiler.SyntaxTree
{
    public class FieldDeclarationExpression : Node
    {
        public readonly string Name;
        public readonly Reflection.DeclaredMember Member;

        public FieldDeclarationExpression(string name, Reflection.DeclaredMember member)
        {
            Name = name;
            Member = member;
        }
    }
}
