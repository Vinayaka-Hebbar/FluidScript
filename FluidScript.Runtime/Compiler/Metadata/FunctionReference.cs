using FluidScript.Compiler.Emit;
using System.Linq;

namespace FluidScript.Compiler.Metadata
{
#if Runtime
    public sealed class FunctionReference : RuntimeObject, IFunctionReference
    {
        public ArgumentType[] Arguments { get; }
        public RuntimeType ReturnType { get; }
        public object Target { get; }

        public System.Reflection.MethodInfo MethodInfo { get; }

        public FunctionReference(object target, ArgumentType[] types, RuntimeType returnType, System.Reflection.MethodInfo method)
        {
            Target = target;
            Arguments = types;
            ReturnType = returnType;
            MethodInfo = method;
        }

        public override RuntimeType ReflectedType => RuntimeType.Function;

        public override string ToString()
        {
            return string.Concat("(", string.Join(",", Arguments.Select(type => type.ToString())), ") => ", ReturnType.ToString());
        }

        public override RuntimeObject DynamicInvoke(RuntimeObject[] args)
        {
            var parameters = Reflection.DeclaredMethod.GetParameters(Arguments, args).ToArray();
            return (RuntimeObject)MethodInfo.Invoke(Target, parameters);
        }
    }
#endif
}
