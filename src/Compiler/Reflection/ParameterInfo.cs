using FluidScript.Compiler.SyntaxTree;

namespace FluidScript.Compiler.Reflection
{
    public struct ParameterInfo
    {
        public static readonly ParameterInfo[] Empty = new ParameterInfo[0];

        public readonly string Name;
        public readonly System.Type Type;
        public Expression DefaultValue { get; set; }

        public ParameterInfo(string name, System.Type type)
        {
            Name = name;
            Type = type;
            DefaultValue = null;
        }
    }
}
