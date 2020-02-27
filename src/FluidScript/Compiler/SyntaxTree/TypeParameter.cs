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

        public Compiler.Emit.ParameterInfo GetParameterInfo(Compiler.ITypeProvider provider)
        {
            if (Type == null)
                return new Compiler.Emit.ParameterInfo(Name, Index, TypeProvider.ObjectType, IsVar);
            return new Compiler.Emit.ParameterInfo(Name, Index, Type.GetType(provider), IsVar);

        }
    }
}
