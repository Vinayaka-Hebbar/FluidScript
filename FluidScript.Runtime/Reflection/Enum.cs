namespace FluidScript.Reflection
{
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
