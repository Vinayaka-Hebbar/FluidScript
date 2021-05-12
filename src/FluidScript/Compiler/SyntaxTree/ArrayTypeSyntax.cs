namespace FluidScript.Compiler.SyntaxTree
{
    public class ArrayTypeSyntax : TypeSyntax
    {
        public readonly TypeSyntax ElementType;

        public readonly int Rank;

        public ArrayTypeSyntax(TypeSyntax elementType, int rank)
        {
            ElementType = elementType;
            Rank = rank;
        }

        public override System.Type ResolveType(ITypeContext provider)
        {
            System.Type elementType = ElementType.ResolveType(provider);
            if (elementType is Emit.IType)
                return new Generators.TypeBuilderInstantiation(TypeProvider.ArrayType, elementType);
            return TypeProvider.ArrayType.MakeGenericType(elementType);
        }

        public override string ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder(ElementType.ToString());
            for (int i = 0; i < Rank; i++)
            {
                sb.Append("[]");
            }
            return sb.ToString();
        }
    }
}
