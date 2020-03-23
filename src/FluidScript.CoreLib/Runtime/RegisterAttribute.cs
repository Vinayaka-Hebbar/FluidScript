namespace FluidScript.Runtime
{
    /// <summary>
    /// Runtime name of a field, property, method, interface or type
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct | System.AttributeTargets.Interface | System.AttributeTargets.Property | System.AttributeTargets.Method | System.AttributeTargets.Field, AllowMultiple = false)]
    public sealed class RegisterAttribute : System.Attribute
    {
        /// <summary>
        /// Name 
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Register type with specified name
        /// </summary>
        /// <param name="name">Name used in runtime</param>
        public RegisterAttribute(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Register type with specified name and implementation
        /// </summary>
        /// <param name="name">Name used in runtime</param>
        /// <param name="implOption">Implement Type</param>
        public RegisterAttribute(string name, RegisterImplOption implOption)
        {
            Name = name;
            ImplOption = implOption;
        }

        public RegisterImplOption ImplOption { get;}

        /// <summary>
        /// Should Method can be used
        /// </summary>
        public bool Deprecated { get; set; }

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
