using FluidScript.Compiler.Emit;
using System;

namespace FluidScript.Compiler.Metadata
{
#if Runtime
    public sealed class DynamicFunction : RuntimeObject, IFunctionReference
    {
        public readonly Reflection.DeclaredMethod DeclaredMethod;
        public readonly RuntimeObject Target;

        public System.Reflection.MethodInfo MethodInfo { get; }

        public DynamicFunction(Reflection.DeclaredMethod declaredMethod, RuntimeObject target, Func<RuntimeObject, RuntimeObject[], RuntimeObject> method)
        {
            DeclaredMethod = declaredMethod;
            Target = target;
            MethodInfo = method.Method;
        }

        public override RuntimeType ReflectedType => RuntimeType.Function;

        public ArgumentType[] Arguments => DeclaredMethod.Arguments;

        public RuntimeType ReturnType => DeclaredMethod.ReturnType;

        public override string ToString()
        {
            return DeclaredMethod.ToString();
        }

        public override RuntimeObject DynamicInvoke(RuntimeObject[] args)
        {
            return (RuntimeObject)MethodInfo.Invoke(DeclaredMethod, new object[] { Target, args });
        }
    }
#endif
}
