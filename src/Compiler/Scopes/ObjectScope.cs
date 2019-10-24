using FluidScript.Compiler.Reflection;
using System.Reflection;

namespace FluidScript.Compiler.Scopes
{
    public class ObjectScope : Scope
    {
        public readonly string Name;
        public TypeInfo BaseType { get; set; }
        public readonly System.Reflection.TypeAttributes Attributes;
        private TypeInfo type;

        public ObjectScope(SyntaxVisitor visitor, string name, TypeAttributes attributes) : base(visitor)
        {
            this.Name = name;
            Attributes = attributes;
        }

        public override TypeInfo GetTypeInfo()
        {
            if(type == null)
            {
                type = new TypeInfo(Name, Attributes, GetModuleInfo(), BaseType);
            }
            return type;
        }

        protected override void Build()
        {
            throw new System.NotImplementedException();
        }
    }
}
