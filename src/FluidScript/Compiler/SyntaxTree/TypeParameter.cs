namespace FluidScript.Compiler.SyntaxTree
{
    public class TypeParameter : Node
    {
        public readonly string Name;

        public readonly TypeSyntax Type;

        public readonly int Index;

        public Expression DefaultValue { get; set; }

        public readonly bool IsVar;

        public TypeParameter(string name, TypeSyntax type, int index, bool isVar)
        {
            Name = name;
            Type = type;
            Index = index;
            IsVar = isVar;
        }

        public override string ToString()
        {
            return string.Concat(Name, ":", Type == null ? "any" : Type.ToString());
        }

        public Reflection.ParameterInfo GetParameterInfo()
        {
            return new Reflection.ParameterInfo(Name, Type.GetTypeInfo(), Index, IsVar);
        }

        public Reflection.Emit.ParameterInfo GetParameterInfo(Reflection.Emit.ITypeProvider provider)
        {
            if (Type == null)
                return new Reflection.Emit.ParameterInfo(Name, Index, Utils.TypeUtils.ObjectType, IsVar);
            return new Reflection.Emit.ParameterInfo(Name, Index, Type.GetType(provider), IsVar);

        }
    }
}
