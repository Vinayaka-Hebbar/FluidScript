namespace FluidScript.Compiler.Reflection
{
    public abstract class DeclaredMember
    {
        public readonly string Name;

        public abstract System.Reflection.MemberTypes MemberType { get; }

        protected DeclaredMember(string name)
        {
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
