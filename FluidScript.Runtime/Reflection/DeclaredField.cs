namespace FluidScript.Reflection
{
    public sealed class DeclaredField : DeclaredMember
    {
        private const System.Reflection.FieldAttributes Default = System.Reflection.FieldAttributes.Public;

        public Compiler.SyntaxTree.Expression ValueAtTop;


        public readonly ITypeInfo Type;

        public DeclaredField(string name, ITypeInfo type) : base(name)
        {
            Type = type;
        }

#if Runtime
        public DeclaredField(string name, RuntimeType type) : base(name)
        {
            reflectedType = type;
        }

        public System.Reflection.FieldAttributes Attributes { get; internal set; } = Default;

        public bool IsReadOnly => (Attributes & System.Reflection.FieldAttributes.InitOnly) == System.Reflection.FieldAttributes.InitOnly;

        public bool IsStatic => (Attributes & System.Reflection.FieldAttributes.Static) == System.Reflection.FieldAttributes.Static;

        public override System.Reflection.MemberTypes MemberType { get; } = System.Reflection.MemberTypes.Field;

        private RuntimeType reflectedType;

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
                if (reflectedType == RuntimeType.Any && value != RuntimeType.Any)
                {
                    reflectedType = value;
                }
            }
        }

        public RuntimeObject DefaultValue;

        internal RuntimeObject Evaluate(RuntimeObject instance)
        {
            RuntimeObject value = ValueAtTop == null ? RuntimeObject.Null : ValueAtTop.Evaluate(instance);
            ReflectedType = value.ReflectedType;
            return value;
        }
#endif
    }
}
