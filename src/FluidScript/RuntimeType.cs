namespace FluidScript
{
    [System.Flags]
    public enum RuntimeType : ushort
    {
        Undefined = 1,
        Any = 0,
        String = 2,
        Double = 4,
        Float = Double | 8,
        Int64 = 16 | Float,
        Int32 = 32 | Int64,
        Int16 = Int32 | 64,
        Char = 128 | Int32,
        /// <summary>
        /// sbyte
        /// </summary>
        Byte = 256 | Int16,
        Bool = 512,
        Array = 1024,
        Function = 2048,
        Void = 4092
    }

}
