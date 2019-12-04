using System;

namespace FluidScript.Library
{
#if Runtime
    internal sealed class DynamicFunction : RuntimeObject, IFunctionReference
    {
        public readonly Reflection.DeclaredMethod DeclaredMethod;
        public readonly object Target;

        public System.Reflection.MethodInfo MethodInfo { get; }

        public DynamicFunction(Reflection.DeclaredMethod declaredMethod, object target, Func<RuntimeObject, RuntimeObject[], RuntimeObject> method)
        {
            DeclaredMethod = declaredMethod;
            Target = target;
            MethodInfo = method.Method;
        }

        public override RuntimeType ReflectedType => RuntimeType.Function;

        public Reflection.ParameterInfo[] Arguments => DeclaredMethod.Arguments;

        public Reflection.ITypeInfo ReturnType => DeclaredMethod.ReturnType;

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
