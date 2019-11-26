using FluidScript.Compiler.Emit;
using System;

namespace FluidScript.Core
{
#if Runtime
    internal sealed class DynamicFunction : RuntimeObject, IFunctionReference
    {
        public readonly Compiler.Reflection.DeclaredMethod DeclaredMethod;
        public readonly object Target;

        public System.Reflection.MethodInfo MethodInfo { get; }

        public DynamicFunction(Compiler.Reflection.DeclaredMethod declaredMethod, object target, Func<RuntimeObject, RuntimeObject[], RuntimeObject> method)
        {
            DeclaredMethod = declaredMethod;
            Target = target;
            MethodInfo = method.Method;
        }

        public override RuntimeType ReflectedType => RuntimeType.Function;

        public ArgumentType[] Arguments => DeclaredMethod.Arguments;

        public RuntimeType ReturnType => DeclaredMethod.ReflectedReturnType;

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
