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

        public override System.Type GetType(ITypeProvider provider)
        {
            return ElementType.GetType(provider).MakeArrayType(Rank);
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
