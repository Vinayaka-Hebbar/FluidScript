namespace FluidScript.Compiler.Reflection
{
    public enum VariableAttributes
    {
        Default = 0,
        Argument = 1,
        Constant = 4
    }

    [System.Flags]
    public enum ArgumentFlags
    {
        None = 0,
        Array = 1,
        VarArgs = 2
    }

}
