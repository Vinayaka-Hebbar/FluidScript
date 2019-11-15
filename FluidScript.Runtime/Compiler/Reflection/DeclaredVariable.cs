using System;

namespace FluidScript.Compiler.Reflection
{
    public class DeclaredVariable
    {
        public readonly string Name;

        public readonly int Index;

        public readonly VariableType Type;

        public SyntaxTree.Expression ValueAtTop;

#if Runtime
        public RuntimeObject DefaultValue;
#endif

        public DeclaredVariable(string name, int index)
        {
            Name = name;
            Index = index;
        }

        public DeclaredVariable(string name, int index, VariableType type) : this(name, index)
        {
            Type = type;
        }

#if Runtime
        internal RuntimeObject Evaluate(RuntimeObject instance)
        {
            return ValueAtTop == null ? RuntimeObject.Null : ValueAtTop.Evaluate(instance);
        }
#endif
    }
}
