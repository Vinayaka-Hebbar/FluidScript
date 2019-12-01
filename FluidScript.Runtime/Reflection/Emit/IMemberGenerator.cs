namespace FluidScript.Reflection.Emit
{
    public interface ITypeProvider
    {
        System.Type GetType(string typeName);
    }

    public interface IMemberGenerator
    {
        string Name { get; }
        System.Reflection.MemberInfo MemberInfo { get; }
        System.Reflection.MemberTypes MemberType { get; }
        bool IsStatic { get; }
        void Build();
    }
}