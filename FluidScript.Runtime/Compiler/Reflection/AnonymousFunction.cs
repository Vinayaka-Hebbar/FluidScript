using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluidScript.Compiler.Emit;

namespace FluidScript.Core
{
    public class AnonymousFunction : RuntimeObject, IFunctionReference
    {
        public readonly RuntimeObject Instance;
        public readonly object Target;

        public System.Reflection.MethodInfo MethodInfo { get; }

        public AnonymousFunction(RuntimeObject instance, object target, ArgumentType[] arguments, RuntimeType returnType, Func<RuntimeObject, RuntimeObject[], RuntimeObject> method)
        {
            Instance = instance;
            Target = target;
            Arguments = arguments;
            ReturnType = returnType;
            MethodInfo = method.Method;
        }

        public override RuntimeType ReflectedType => RuntimeType.Function;

        public ArgumentType[] Arguments { get; }

        public RuntimeType ReturnType { get; }

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
