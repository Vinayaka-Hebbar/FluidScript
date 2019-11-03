namespace FluidScript.Compiler.Reflection
{
    public enum VariableType
    {
        Local,
        Argument
    }

    [System.Flags]
    public enum DeclaredFlags
    {
        None = 0,
        Array = 1,
    }
}
