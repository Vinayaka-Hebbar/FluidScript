namespace FluidScript.Compiler.Emit
{
    public struct ArgumentType
    {
        public static readonly ArgumentType Double = new ArgumentType(RuntimeType.Double);
        public static readonly ArgumentType Int32 = new ArgumentType(RuntimeType.Int32);
        public static readonly ArgumentType String = new ArgumentType(RuntimeType.String);
        public static readonly ArgumentType VarArg = new ArgumentType(RuntimeType.Any, ArgumentFlags.VarArg);

        public readonly RuntimeType RuntimeType;
        public readonly ArgumentFlags Flags;

        public ArgumentType(RuntimeType type, ArgumentFlags flags = ArgumentFlags.None)
        {
            RuntimeType = type;
            Flags = flags;
        }
    }

    public enum ArgumentFlags
    {
        None,
        VarArg
    }
}
