namespace FluidScript.Compiler.SyntaxTree
{
    public abstract class MemberDeclaration : Node
    {
        public Modifiers Modifiers { get; internal set; }

        protected MemberDeclaration()
        {
        }

        public abstract void Generate(Generators.TypeGenerator generator);
    }
}
