namespace FluidScript.Compiler.SyntaxTree
{
    public class TypeDeclaration : Declaration
    {
        public readonly string BaseTypeName;
        public readonly string[] Implements;

        public TypeDeclaration(string name, string baseTypeName, string[] implements) : base(name)
        {
            BaseTypeName = baseTypeName;
            Implements = implements;
        }
    }
}
