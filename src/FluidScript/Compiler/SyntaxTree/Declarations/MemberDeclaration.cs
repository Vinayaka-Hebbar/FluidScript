namespace FluidScript.Compiler.SyntaxTree
{
    public abstract class MemberDeclaration : Node
    {
        public Modifiers Modifiers { get; set; }

        public DeclarationType DeclarationType { get; }

        protected MemberDeclaration(DeclarationType declarationType)
        {
            DeclarationType = declarationType;
        }

        public abstract void CreateMember(Generators.TypeGenerator generator);
    }
}
