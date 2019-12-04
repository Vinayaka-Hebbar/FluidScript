namespace FluidScript.Library
{
    public interface IFunctionReference
    {
        Reflection.ParameterInfo[] Arguments { get; }
        RuntimeType ReflectedType { get; }
        Reflection.ITypeInfo ReturnType { get; }

        System.Reflection.MethodInfo MethodInfo { get; }

#if Runtime
        RuntimeObject DynamicInvoke(RuntimeObject[] args);
#endif
    }
}