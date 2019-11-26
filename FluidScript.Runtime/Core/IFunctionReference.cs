using FluidScript.Compiler.Emit;

namespace FluidScript.Core
{
    public interface IFunctionReference
    {
        ArgumentType[] Arguments { get; }
        RuntimeType ReflectedType { get; }
        RuntimeType ReturnType { get; }

        System.Reflection.MethodInfo MethodInfo { get; }

#if Runtime
        RuntimeObject DynamicInvoke(RuntimeObject[] args);
#endif
    }
}