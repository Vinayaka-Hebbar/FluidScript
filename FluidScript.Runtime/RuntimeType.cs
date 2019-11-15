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
        Unsigned = 32,
        Int64 = 16 | Float,
        UInt64 = Unsigned | Float,
        Int32 = 64 | Int64,
        UInt32 = 128 | Int64 | UInt64,
        Int16 = Int32 | 256,
        UInt16 = Int32 | UInt32,
        Char = 256 | Int32 | UInt16,
        /// <summary>
        /// sbyte
        /// </summary>
        Byte = 512 | Int16,
        /// <summary>
        /// byte
        /// </summary>
        UByte = 512 | UInt16 | Int16,
        Bool = 1024,
        Array = 2048,
        Function = 4096,
        Void = 8192
    }

}
