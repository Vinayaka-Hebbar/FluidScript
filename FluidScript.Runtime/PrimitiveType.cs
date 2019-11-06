namespace FluidScript
{
    [System.Flags]
    public enum PrimitiveType : short
    {
        Undefined = -1,
        Any = 0,
        Number = 2,
        String = 4,
        Double = Number | 8,
        Float = Double | 16,
        Unsigned = 64,
        Int64 = 32 | Float,
        UInt64 = Unsigned | Float,
        Int32 = 128 | Int64,
        UInt32 = 256 | Int64 | UInt64,
        Int16 = Int32 | 512,
        UInt16 = Int32 | UInt32,
        Char = 512 | Int32 | UInt16,
        /// <summary>
        /// sbyte
        /// </summary>
        Byte = 1024 | Int16,
        /// <summary>
        /// byte
        /// </summary>
        UByte = 1024 | UInt16 | Int16,
        Bool = 2048,
        Array = 4096,
        ConcatenatedString = String | 8192,
    }
}
