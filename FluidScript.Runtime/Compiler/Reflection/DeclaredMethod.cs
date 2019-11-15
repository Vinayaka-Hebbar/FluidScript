using System.Linq;

namespace FluidScript.Compiler.Reflection
{
    //todo member base
    public class DeclaredMethod
    {
        public readonly string Name;
        public readonly int Index;
        public SyntaxTree.FunctionDeclaration Declaration;
        private Emit.ArgumentType[] arguments;
        private RuntimeType returnType = RuntimeType.Undefined;

        public Emit.ArgumentType[] Arguments
        {
            get
            {
                if (arguments == null && Declaration != null)
                    arguments = Declaration.ArgumentTypes().ToArray();
                return arguments;
            }
            private set
            {
                arguments = value;
            }
        }

        public RuntimeType ReturnType
        {
            get
            {
                if (returnType == RuntimeType.Undefined)
                {
                    returnType = Declaration.ReturnType();
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

        public DeclaredMethod(string name, int index)
        {
            Name = name;
            Index = index;
        }

        public DeclaredMethod(string name, int index, Emit.ArgumentType[] types, RuntimeType returnType)
        {
            Name = name;
            Index = index;
            Arguments = types;
            ReturnType = returnType;
        }

#if Runtime
        internal RuntimeObject DynamicInvoke(RuntimeObject obj, RuntimeObject[] args)
        {
            var instance = Declaration.Prototype.CreateInstance(obj);
            for (int index = 0; index < Arguments.Length; index++)
            {
                var arg = Arguments[index];
                instance[arg.Name] = arg.IsVarArgs() ? new Core.ArrayObject(args.Skip(index).ToArray(), arg.RuntimeType) : args[index];
            }
            return ValueAtTop.Evaluate(instance);
        }

        public override string ToString()
        {
            return string.Concat(Name, ":(", string.Join(",", Arguments.Select(type => type.ToString())), ") => ", ReturnType.ToString());
        }

        internal static System.Collections.Generic.IEnumerable<object> GetParameters(Emit.ArgumentType[] types, RuntimeObject[] args)
        {
            for (int i = 0; i < types.Length; i++)
            {
                var type = types[i];
                if (type.Flags == DeclaredFlags.VarArgs)
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
