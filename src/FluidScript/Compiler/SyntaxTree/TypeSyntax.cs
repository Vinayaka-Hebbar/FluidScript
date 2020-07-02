namespace FluidScript.Compiler.SyntaxTree
{
    public abstract class TypeSyntax : Node
    {
        private static TypeSyntax m_void;
        public static TypeSyntax Void
        {
            get
            {
                if (m_void == null)
                    m_void = new RefTypeSyntax("Void", null) { Type = TypeProvider.VoidType };
                return m_void;
            }
        }

        public System.Type Type { get; protected set; }

        public static TypeSyntax Create(System.Type type)
        {
            if (type is null)
                return null;
            TypeSyntax value;
            if (type.IsArray)
            {
                value = new ArrayTypeSyntax(Create(type.GetElementType()), type.GetArrayRank());
            }
            else if (type.IsGenericType)
            {
                var types = type.GetGenericArguments();
                NodeList<TypeSyntax> typeNodes = new NodeList<TypeSyntax>(types.Length);
                foreach (var arg in types)
                {
                    typeNodes.Add(Create(arg));
                }
                value = new RefTypeSyntax(type.FullName, typeNodes);
            }
            else
            {
                value = new RefTypeSyntax(type.FullName);
            }
            value.Type = type;
            return value;
        }

        public abstract System.Type ResolveType(ITypeContext provider);
    }
}
