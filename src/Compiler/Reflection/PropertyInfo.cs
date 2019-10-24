
using System.Reflection;
using System.Reflection.Emit;

namespace FluidScript.Compiler.Reflection
{
    public class PropertyInfo : MemberInfo, IReflection
    {
        public readonly string Name;
        public readonly TypeInfo PropertyType;
        public MethodInfo Getter { get; set; }
        public MethodInfo Setter { get; set; }
        public readonly System.Reflection.PropertyAttributes Attributes;

        public PropertyInfo(string name, TypeInfo propertyType, System.Reflection.PropertyAttributes attributes) : base(declaredType)
        {
            Name = name;
            PropertyType = propertyType;
            Attributes = attributes;
        }

        public PropertyInfo(string name, TypeInfo propertyType, TypeInfo declaredType, MethodInfo getter, MethodInfo setter, System.Reflection.PropertyAttributes attributes) : base(declaredType)
        {
            Name = name;
            PropertyType = propertyType;
            Getter = getter;
            Setter = setter;
            Attributes = attributes;
        }

        public bool CanRead { get; }

        public override MemberTypes Types => MemberTypes.Property;

        public void Generate(TypeBuilder builder)
        {
           //todo
        }
    }
}
