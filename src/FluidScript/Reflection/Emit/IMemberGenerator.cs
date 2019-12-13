namespace FluidScript.Reflection.Emit
{
    /// <summary>
    /// Resolve type
    /// </summary>
    public interface ITypeProvider
    {
        /// <summary>
        /// Get resolved <see cref="System.Type"/>
        /// </summary>
        System.Type GetType(TypeName typeName);
    }

    /// <summary>
    /// Member Generate
    /// </summary>
    public interface IMemberGenerator
    {
        string Name { get; }
        System.Reflection.MemberInfo MemberInfo { get; }
        System.Reflection.MemberTypes MemberType { get; }
        bool IsStatic { get; }
        void Build();
        bool BindingFlagsMatch(System.Reflection.BindingFlags flags);
    }
}