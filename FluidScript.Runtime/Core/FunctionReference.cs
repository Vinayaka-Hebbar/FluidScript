using System.Linq;

namespace FluidScript.Library
{
#if Runtime
    public sealed class FunctionReference : RuntimeObject, IFunctionReference
    {
        private static Compiler.Metadata.Prototype prototype;
        public Reflection.ParameterInfo[] Arguments { get; }
        public Reflection.ITypeInfo ReturnType { get; }
        public object Target { get; }

        public System.Reflection.MethodInfo MethodInfo { get; }

        public FunctionReference(object target, Reflection.ParameterInfo[] arguments, Reflection.ITypeInfo returnType, System.Reflection.MethodInfo method)
        {
            Target = target;
            Arguments = arguments;
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

        public override Compiler.Metadata.Prototype GetPrototype()
        {
            if (prototype is null)
            {
                var baseProto = new Compiler.Metadata.DefaultObjectPrototype(new Reflection.DeclaredMethod[0]);
                var methods = Reflection.TypeHelper.GetMethods(GetType());
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
