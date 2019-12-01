namespace FluidScript.Reflection
{
    [System.Flags]
    public enum VariableFlags
    {
        Default = 0,
        Argument = 1,
        Constant = 4
    }

    [System.Flags]
    public enum TypeFlags
    {
        None = 0,
        Array = 1
    }

    [System.Flags]
    public enum ArgumentFlags
    {
        None,
        VarArgs
    }

    [System.Flags]
    public enum Modifiers
    {
        None = 0,
        Private = 2,
        Abstract = 4,
        Implement = 8,
        Static = 16,
        ReadOnly = 32,
        Getter = 64,
        Setter = 128
    }

}
