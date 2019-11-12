using System;
using System.Linq;

namespace FluidScript.Compiler.Reflection
{
    public class DeclaredMethod
    {
        public readonly string Name;
        public readonly int Index;
        public SyntaxTree.FunctionDeclaration Declaration;
        private Emit.ArgumentType[] types;
        public Emit.ArgumentType[] Types
        {
            get
            {
                if (types == null && Declaration != null)
                    types = Declaration.ArgumentTypes().ToArray();
                return types;
            }
            set
            {
                types = value;
            }
        }

        public SyntaxTree.BlockStatement ValueAtTop;

        public System.Func<RuntimeObject[], RuntimeObject> Delegate;

        public DeclaredMethod(string name, int index)
        {
            Name = name;
            Index = index;
        }

        public DeclaredMethod(string name, int index, Emit.ArgumentType[] types)
        {
            Name = name;
            Index = index;
            Types = types;
        }

        internal Func<RuntimeObject[], RuntimeObject> Create(Metadata.Prototype prototype)
        {
            if (ValueAtTop != null)
            {
                return (args) =>
                {
                    for (int i = 0; i < Declaration.Arguments.Length; i++)
                    {
                        prototype.DefineVariable(Declaration.Arguments[i].Name, args[i]);
                    }
                    return ValueAtTop.Evaluate();
                };
            }

            return (args) => { return RuntimeObject.Null; };
        }


        internal RuntimeObject Exec(object instance, System.Reflection.MethodInfo method, RuntimeObject[] args)
        {
            var parameters = GetParameters(Types, args).ToArray();
            return (RuntimeObject)method.Invoke(instance, parameters);
        }

        private static System.Collections.Generic.IEnumerable<object> GetParameters(Emit.ArgumentType[] types, RuntimeObject[] args)
        {
            for (int i = 0; i < types.Length; i++)
            {
                var type = types[i];
                if (type.Flags == Emit.ArgumentFlags.VarArg)
                {
                    yield return args.Skip(i).ToArray();
                }
                else
                {
                    yield return args[i];
                }
            }
        }
    }
}
