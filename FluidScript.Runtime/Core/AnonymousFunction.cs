using FluidScript.Reflection.Emit;
using System;
using System.Linq;

namespace FluidScript.Library
{
    public class AnonymousFunction : RuntimeObject, IFunctionReference
    {
        public readonly RuntimeObject Instance;
        public readonly object Target;

        public System.Reflection.MethodInfo MethodInfo { get; }

        public AnonymousFunction(RuntimeObject instance, object target, Reflection.ParameterInfo[] arguments, Reflection.ITypeInfo returnType, Func<RuntimeObject, RuntimeObject[], RuntimeObject> method)
        {
            Instance = instance;
            Target = target;
            Arguments = arguments;
            ReturnType = returnType;
            MethodInfo = method.Method;
        }

        public override RuntimeType ReflectedType => RuntimeType.Function;

        public Reflection.ParameterInfo[] Arguments { get; }

        public Reflection.ITypeInfo ReturnType { get; }

        public override string ToString()
        {
            return string.Concat("(", string.Join(",", Arguments.Select(arg => arg.ToString())), "):", ReturnType.ToString());
        }

        public override RuntimeObject DynamicInvoke(RuntimeObject[] args)
        {
            return (RuntimeObject)MethodInfo.Invoke(Target, new object[] { Instance, args });
        }
    }
}
