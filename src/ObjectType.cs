namespace FluidScript
{
    [System.Flags]
    public enum ObjectType : uint
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
        Function = 8192,
        ConcatenatedString = String | 16384,
        Inbuilt = 32768,
        //Return
        Void = 65536,
    }
}
