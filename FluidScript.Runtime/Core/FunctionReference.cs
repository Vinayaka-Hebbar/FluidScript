using FluidScript.Compiler.Emit;
using System.Collections.Generic;
using System.Linq;

namespace FluidScript.Core
{
#if Runtime
    public sealed class FunctionReference : RuntimeObject, IFunctionReference
    {
        private static Compiler.Metadata.Prototype prototype;
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
            var parameters = Compiler.Reflection.DeclaredMethod.GetParameters(Arguments, args).ToArray();
            return (RuntimeObject)MethodInfo.Invoke(Target, parameters);
        }

        public override Compiler.Metadata.Prototype GetPrototype()
        {
            if (prototype is null)
            {
                var baseProto = new Compiler.Metadata.DefaultObjectPrototype(new Compiler.Reflection.DeclaredMethod[0]);
                var methods = Compiler.Reflection.TypeHelper.GetMethods(GetType());
                prototype = new Compiler.Metadata.ObjectPrototype(methods, null, baseProto, "FunctionReference")
                {
                    IsSealed = true
                };
            }
            return prototype;
        }
    }
#endif
}
