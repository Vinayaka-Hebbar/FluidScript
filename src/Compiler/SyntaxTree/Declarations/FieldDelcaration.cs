namespace FluidScript.Compiler.SyntaxTree
{
    public class FieldDelcaration : Declaration
    {
        public readonly string TypeName;

        public FieldDelcaration(string name, string typeName) : base(name)
        {
            TypeName = typeName;
        }
    }
}
