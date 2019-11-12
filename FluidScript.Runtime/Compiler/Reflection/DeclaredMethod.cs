using System;
using System.Linq;

namespace FluidScript.Compiler.Reflection
{
    public class DeclaredMethod
    {
        public readonly string Name;
        public readonly int Index;
        public SyntaxTree.FunctionDeclaration Declaration;
        private PrimitiveType[] types;
        public PrimitiveType[] Types
        {
            get
            {
                if (types == null && Declaration != null)
                    types = Declaration.PrimitiveArguments().ToArray();
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

        public DeclaredMethod(string name, int index, PrimitiveType[] types)
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
    }
}
