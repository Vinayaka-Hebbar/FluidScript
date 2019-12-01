using System.Linq;

namespace FluidScript.Reflection
{
    //todo member base
    public sealed class DeclaredMethod : DeclaredMember
    {
        public System.Reflection.MethodAttributes Attributes { get; internal set; }

        public readonly ParameterInfo[] Arguments;

        private RuntimeType returnType = RuntimeType.Undefined;

        public readonly ITypeInfo ReturnType;

        public Compiler.Metadata.Prototype DeclaredPrototype { get; internal set; }


        public RuntimeType ReflectedReturnType
        {
            get
            {
                if (returnType == RuntimeType.Undefined)
                {
                    returnType = ReturnType.RuntimeType;
                }
                return returnType;
            }
            private set
            {
                returnType = value;
            }
        }

        public Compiler.SyntaxTree.BlockStatement ValueAtTop;

        public System.Reflection.MethodInfo Store;

        public DeclaredMethod(string name, ParameterInfo[] arguments, ITypeInfo returnType) : base(name)
        {
            Arguments = arguments;
            ReturnType = returnType;
        }

        public bool IsStatic => (Attributes & System.Reflection.MethodAttributes.Static) == System.Reflection.MethodAttributes.Static;

        public override System.Reflection.MemberTypes MemberType { get; } = System.Reflection.MemberTypes.Method;

#if Runtime

        /// <summary>
        /// Static Data
        /// </summary>
        internal Core.IFunctionReference Default;

        internal RuntimeObject DynamicInvoke(RuntimeObject obj, RuntimeObject[] args)
        {
            var prototype = new Compiler.Metadata.FunctionPrototype(obj.GetPrototype());
            //todo default value arg
            var instance = new Core.LocalInstance(prototype, obj);
            for (int index = 0; index < Arguments.Length; index++)
            {
                var arg = Arguments[index];
                instance[arg.Name] = arg.IsVar ? new Library.ArrayObject(args.Skip(index).ToArray(), arg.Type.RuntimeType) : args[index];
            }
            return ValueAtTop.Evaluate(instance);
        }

        public override string ToString()
        {
            return string.Concat("(", string.Join(",", Arguments.Select(type => type.ToString())), ") => ", ReflectedReturnType.ToString());
        }

        internal static System.Collections.Generic.IEnumerable<object> GetParameters(ParameterInfo[] types, RuntimeObject[] args)
        {
            for (int i = 0; i < types.Length; i++)
            {
                var type = types[i];
                if (type.IsVar)
                {
                    yield return args.Skip(i).ToArray();
                }
                else
                {
                    yield return args[i];
                }
            }
        }
#endif
    }
}
