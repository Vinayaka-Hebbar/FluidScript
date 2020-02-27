namespace FluidScript.Runtime
{
    /// <summary>
    /// Runtime name of a property or method
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Property | System.AttributeTargets.Method)]
    public sealed class RegisterAttribute : System.Attribute
    {
        /// <summary>
        /// Name 
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        public RegisterAttribute(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Should Method name is same as <see cref="Name"/>
        /// </summary>
        public bool DoGenerate { get; set; }

        ///<inheritdoc/>
        public override bool Match(object obj)
        {
            return Name.Equals(obj);
        }

        ///<inheritdoc/>
        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        ///<inheritdoc/>
        public override bool Equals(object obj)
        {
            return Name.Equals(obj);
        }

        ///<inheritdoc/>
        public override string ToString()
        {
            return Name;
        }
    }
}
