using System.Linq;

namespace FluidScript.Compiler.Reflection
{
    //todo member base
    public sealed class DeclaredMethod : DeclaredMember
    {
        public System.Reflection.MethodAttributes Attributes { get; internal set; }

        private Emit.ArgumentType[] arguments;
        private RuntimeType returnType = RuntimeType.Undefined;

        internal Metadata.FunctionPrototype Prototype;

        public readonly Emit.TypeName ReturnType;

        public readonly SyntaxTree.ArgumentInfo[] ArgumentInfos;

        public Emit.ArgumentType[] Arguments
        {
            get
            {
                if (arguments == null)
                {
                    if (ArgumentInfos == null)
                        arguments = new Emit.ArgumentType[0];
                    else
                        arguments = ArgumentTypes().ToArray();
                }
                return arguments;
            }
            private set
            {
                arguments = value;
            }
        }

        public System.Collections.Generic.IEnumerable<Emit.ArgumentType> ArgumentTypes()
        {
            foreach (var arg in ArgumentInfos)
            {
                yield return new Emit.ArgumentType(arg.Name, arg.TypeName);
            }
        }

        public RuntimeType ReflectedReturnType
        {
            get
            {
                if (returnType == RuntimeType.Undefined)
                {
                    returnType = ReturnType.GetRuntimeType();
                }
                return returnType;
            }
            private set
            {
                returnType = value;
            }
        }

        public SyntaxTree.BodyStatement ValueAtTop;

        public System.Reflection.MethodInfo Store;

        public DeclaredMethod(string name, SyntaxTree.ArgumentInfo[] arguments, Emit.TypeName returnType) : base(name)
        {
            ArgumentInfos = arguments;
            ReturnType = returnType;
        }

        public DeclaredMethod(string name, Emit.ArgumentType[] types, RuntimeType returnType) : base(name)
        {
            Arguments = types;
            ReflectedReturnType = returnType;
        }

        public bool IsStatic => (Attributes & System.Reflection.MethodAttributes.Static) == System.Reflection.MethodAttributes.Static;

        public override System.Reflection.MemberTypes MemberType { get; } = System.Reflection.MemberTypes.Method;

#if Runtime

        /// <summary>
        /// Static Data
        /// </summary>
        internal Metadata.IFunctionReference Default;

        internal RuntimeObject DynamicInvoke(RuntimeObject obj, RuntimeObject[] args)
        {
            //todo default value arg
            var instance = Prototype.CreateInstance(obj);
            for (int index = 0; index < Arguments.Length; index++)
            {
                var arg = Arguments[index];
                instance[arg.Name] = arg.IsVarArgs() ? new Library.ArrayObject(args.Skip(index).ToArray(), arg.RuntimeType) : args[index];
            }
            return ValueAtTop.Evaluate(instance);
        }

        public override string ToString()
        {
            return string.Concat("(", string.Join(",", Arguments.Select(type => type.ToString())), ") => ", ReflectedReturnType.ToString());
        }

        internal static System.Collections.Generic.IEnumerable<object> GetParameters(Emit.ArgumentType[] types, RuntimeObject[] args)
        {
            for (int i = 0; i < types.Length; i++)
            {
                var type = types[i];
                if (type.Flags == ArgumentFlags.VarArgs)
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
