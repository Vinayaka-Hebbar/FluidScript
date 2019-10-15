namespace FluidScript
{
    [System.Flags]
    public enum ObjectType : ushort
    {
        Default = 1,
        Integer = 2,
        Float = 4,
        Double = 8,
        Char = 16,
        String = 32,
        Bool = 64,
        Number = 128,
        Array = 256,
        Object = 512,
        Function = 1024,
        //Return
        Void = 2048,
        Inbuilt = 4096
    }
}
