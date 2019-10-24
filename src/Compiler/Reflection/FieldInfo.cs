using System.Reflection;

namespace FluidScript.Compiler.Reflection
{
    public class FieldInfo : MemberInfo, IReflection
    {
        public readonly string Name;
        public readonly TypeInfo Type;
        public readonly System.Reflection.FieldAttributes Attributes;

        public FieldInfo(string name, TypeInfo type, System.Reflection.FieldAttributes attributes) : base()
        {
            Name = name;
            Type = type;
            Attributes = attributes;
        }

        public FieldInfo(string name, TypeInfo type, TypeInfo declaredType, System.Reflection.FieldAttributes attributes) : base(declaredType)
        {
            Name = name;
            Type = type;
            Attributes = attributes;
        }

        public object Value { get; set; }

        public override MemberTypes Types => MemberTypes.Field;

        public void Generate(System.Reflection.Emit.TypeBuilder builder)
        {
            var field = builder.DefineField(Name, Type.RuntimeType(), Attributes);
        }
    }
}
