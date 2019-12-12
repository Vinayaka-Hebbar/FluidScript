namespace FluidScript.Runtime
{
    [System.AttributeUsage(System.AttributeTargets.Property | System.AttributeTargets.Method)]
    public sealed class RegisterAttribute : System.Attribute
    {
        public readonly string Name;

        public RegisterAttribute(string name)
        {
            Name = name;
        }

        public bool DoGenerate { get; set; }

        public override bool Match(object obj)
        {
            return obj.Equals(Name);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Name.Equals(obj);
        }
    }
}
