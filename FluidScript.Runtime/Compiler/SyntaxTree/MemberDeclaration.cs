namespace FluidScript.Compiler.SyntaxTree
{
    public abstract class MemberDeclaration : Node
    {
        public Reflection.Modifiers Modifiers { get; internal set; }

        protected MemberDeclaration()
        {
        }

        public abstract void Create(Reflection.Emit.TypeGenerator generator);
    }
}
