namespace FluidScript.Compiler.Emit
{
    /// <summary>
    /// Member Generate
    /// </summary>
    public interface IMemberGenerator
    {
        string Name { get; }
        System.Reflection.MemberInfo MemberInfo { get; }

        System.Type DeclaringType { get; }
        System.Reflection.MemberTypes MemberType { get; }
        bool IsStatic { get; }
        void Generate();
        bool BindingFlagsMatch(System.Reflection.BindingFlags flags);
    }
}