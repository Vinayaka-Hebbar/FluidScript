namespace FluidScript
{
    [System.Flags]
    public enum PrimitiveType : uint
    {
        Object = 0,
        Null = 1,
        Number = 2,
        String = 4,
        Double = Number | 8,
        Float = Double | 16,
        Unsigned = 32,
        Int64 = Float | 64,
        UInt64 = Float | Unsigned,
        Int32 = Int64 | 128,
        UInt32 = Int64 | Unsigned,
        Char = Int32 | 256,
        Int16 = Int32 | 512,
        UInt16 = Int32 | Unsigned,
        Byte = Int16 | 1024,
        UByte = Int16 | Unsigned,
        Bool = 2048,
        Array = 4096,
        ConcatenatedString = String | 8192,
        //Return
        Any = 16384,
    }
}
