namespace FluidScript.Compiler.SyntaxTree
{
    public struct ArgumentInfo
    {
        public static readonly ArgumentInfo[] Empty = new ArgumentInfo[0];

        public readonly string Name;

        public readonly Emit.TypeName TypeName;

        public Expression DefaultValue { get; set; }

        public ArgumentInfo(string name, Emit.TypeName typeName)
        {
            Name = name;
            TypeName = typeName;
            DefaultValue = null;
        }

        public override string ToString()
        {
            return string.Format("{0}:{1}", Name, TypeName);
        }
    }
}
