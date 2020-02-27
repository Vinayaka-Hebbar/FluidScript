namespace FluidScript.Compiler.SyntaxTree
{
    public class ArrayTypeSyntax : TypeSyntax
    {
        public readonly TypeSyntax ElementType;

        public readonly NodeList<Expression> Ranks;

        public ArrayTypeSyntax(TypeSyntax elementType, NodeList<Expression> sizes)
        {
            ElementType = elementType;
            Ranks = sizes;
        }

        public override System.Type GetType(ITypeProvider provider)
        {
            return ElementType.GetType(provider).MakeArrayType();
        }

        public override string ToString()
        {
            return string.Concat(ElementType.ToString(), "[", string.Join(",", System.Linq.Enumerable.Select(Ranks, size => size.ToString())), "]");
        }
    }
}
