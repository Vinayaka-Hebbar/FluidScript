using FluidScript.Compiler.Emit;

namespace FluidScript.Compiler.SyntaxTree
{
    public class TypeParameter : Node
    {
        public readonly string Name;

        public readonly TypeSyntax Type;

        public readonly int Index;

        public Expression DefaultValue { get; set; }

        public readonly bool IsVarArgs;

        public TypeParameter(string name, TypeSyntax type, int index, bool isVarArgs)
        {
            Name = name;
            Type = type;
            Index = index;
            IsVarArgs = isVarArgs;
        }

        public TypeParameter(ParameterInfo parameter)
        {
            Name = parameter.Name;
            Type = TypeSyntax.Create(parameter.Type);
            Index = parameter.Index;
            IsVarArgs = parameter.IsVarArgs;
        }

        public override string ToString()
        {
            return string.Concat(Name, ":", Type == null ? "any" : Type.ToString());
        }

        public ParameterInfo GetParameterInfo(ITypeContext provider)
        {
            if (Type == null)
                return new ParameterInfo(Name, Index, TypeProvider.AnyType, IsVarArgs);
            return new ParameterInfo(Name, Index, Type.ResolveType(provider), IsVarArgs);

        }
    }
}
