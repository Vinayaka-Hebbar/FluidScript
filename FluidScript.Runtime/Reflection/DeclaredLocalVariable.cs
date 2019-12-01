namespace FluidScript.Reflection
{
    public sealed class DeclaredLocalVariable
    {
        public readonly string Name;

        public readonly ITypeInfo Type;

        public readonly int Index;

        public Compiler.SyntaxTree.Expression ValueAtTop;

        public readonly VariableFlags Attributes;

        public DeclaredLocalVariable(string name, ITypeInfo type, int index, VariableFlags attribute)
        {
            Name = name;
            Type = type;
            Index = index;
            Attributes = attribute;
        }

        public DeclaredLocalVariable(string name, RuntimeType type, int index, VariableFlags attribute)
        {
            Name = name;
            reflectedType = type;
            Index = index;
            Attributes = attribute;
        }

#if Runtime

        private RuntimeType reflectedType = RuntimeType.Undefined;
        public RuntimeType ReflectedType
        {
            get
            {
                if (reflectedType == RuntimeType.Undefined)
                {
                    reflectedType = Type.RuntimeType;
                }
                return reflectedType;
            }
            set
            {
                if (reflectedType == RuntimeType.Any)
                {
                    reflectedType = value;
                }
            }
        }

        public RuntimeObject DefaultValue;

        internal RuntimeObject Evaluate(RuntimeObject instance)
        {
            RuntimeObject value = ValueAtTop == null ? RuntimeObject.Null : ValueAtTop.Evaluate(instance);
            reflectedType = value.ReflectedType;
            return value;
        }
#endif
    }
}
