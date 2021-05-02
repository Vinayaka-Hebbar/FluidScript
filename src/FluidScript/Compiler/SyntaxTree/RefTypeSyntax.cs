namespace FluidScript.Compiler.SyntaxTree
{
    public class RefTypeSyntax : TypeSyntax
    {
        public readonly string Name;
        public readonly INodeList<TypeSyntax> GenericPrameters;

        public RefTypeSyntax(string name, INodeList<TypeSyntax> generic)
        {
            Name = name;
            GenericPrameters = generic;
        }

        public RefTypeSyntax(string name)
        {
            Name = name;
        }

        public override System.Type ResolveType(ITypeContext provider)
        {
            if (Type == null)
            {
                if (GenericPrameters == null)
                {
                    Type = provider.GetType(Name);
                }
                else
                {
                    Type = provider.GetType(string.Concat(Name, '`', GenericPrameters.Count));
                    if (Type == null)
                        Type = TypeProvider.AnyType;
                    Type = Type.MakeGenericType(GenericPrameters.Map(p => p.ResolveType(provider)));
                }
            }
            return Type;
        }

        public override string ToString()
        {
            return GenericPrameters == null ?
               Name
               : string.Concat(Name, '<', string.Join(",", GenericPrameters.Map<string>(item => item.ToString())), '>');
        }
    }
}
