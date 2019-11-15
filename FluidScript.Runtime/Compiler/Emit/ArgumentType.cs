using System;

namespace FluidScript.Compiler.Emit
{
    public struct ArgumentType
    {
        public readonly string Name;
        public readonly RuntimeType RuntimeType;
        public readonly Reflection.DeclaredFlags Flags;

        public ArgumentType(string name, RuntimeType type, Reflection.DeclaredFlags flags = Reflection.DeclaredFlags.None)
        {
            Name = name;
            RuntimeType = type;
            Flags = flags;
        }

        public override string ToString()
        {
            return RuntimeType.ToString();
        }

        public bool IsVarArgs()
        {
            return (Flags & Reflection.DeclaredFlags.VarArgs) == Reflection.DeclaredFlags.VarArgs;
        }
    }

    [System.Flags]
    public enum ArgumentTypes
    {
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
        VarArg = 4098
    }
}
