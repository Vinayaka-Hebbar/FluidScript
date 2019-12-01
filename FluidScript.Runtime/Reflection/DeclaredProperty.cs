namespace FluidScript.Reflection
{
    public class DeclaredProperty : DeclaredMember
    {
        public DeclaredProperty(string name, RuntimeType type, DeclaredMethod getter, DeclaredMethod setter) : base(name)
        {
            Type = type;
            Getter = getter;
            Setter = setter;
        }

        public RuntimeType Type;

        public System.Reflection.PropertyAttributes Attributes { get; set; }

        public override System.Reflection.MemberTypes MemberType => System.Reflection.MemberTypes.Property;

        public readonly DeclaredMethod Getter;

        public readonly DeclaredMethod Setter;


    }
}
