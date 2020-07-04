namespace FluidScript.Compiler.Emit
{
    /// <summary>
    /// Emit Parameter
    /// </summary>
    /// Todo: Default value for parameter
    public struct ParameterInfo
    {
        public readonly string Name;
        public readonly int Index;
        public readonly System.Type Type;
        public readonly bool IsVarArgs;

        public ParameterInfo(string name, int index, System.Type type, bool isVarArgs = false)
        {
            Name = name;
            Index = index;
            Type = type;
            IsVarArgs = isVarArgs;
        }
    }
}
