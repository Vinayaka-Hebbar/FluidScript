namespace FluidScript.Compiler.Reflection
{
    public sealed class DeclaredLocalVariable
    {
        public readonly string Name;

        public readonly Emit.TypeName Type;

        public readonly int Index;

        public SyntaxTree.Expression ValueAtTop;

        public readonly VariableAttributes Attributes;

        public DeclaredLocalVariable(string name, Emit.TypeName type, int index, VariableAttributes attribute)
        {
            Name = name;
            Type = type;
            Index = index;
            Attributes = attribute;
        }

        public DeclaredLocalVariable(string name, RuntimeType type, int index, VariableAttributes attribute)
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
                    reflectedType = Type.GetRuntimeType();
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
