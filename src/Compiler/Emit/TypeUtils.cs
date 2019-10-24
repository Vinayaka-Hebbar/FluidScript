using System;

namespace FluidScript.Compiler.Emit
{
    public static class TypeUtils
    {
        public static Type ToType(ObjectType type)
        {
            if (type == ObjectType.Null)
                return typeof(object);
            if (type == ObjectType.Bool)
                return typeof(bool);
            if (type == ObjectType.String)
                return typeof(string);
            if (type == ObjectType.Char)
                return typeof(char);
            bool isNumber = (type & ObjectType.Number) == ObjectType.Number;
            if (isNumber)
            {
                //Unset ObjectType.Number
                switch (type & (~ObjectType.Number))
                {
                    case ObjectType.Byte:
                        return typeof(sbyte);
                    case ObjectType.UByte:
                        return typeof(byte);
                    case ObjectType.Int16:
                        return typeof(short);
                    case ObjectType.UInt16:
                        return typeof(ushort);
                    case ObjectType.Int32:
                        return typeof(int);
                    case ObjectType.UInt32:
                        return typeof(uint);
                    case ObjectType.Int64:
                        return typeof(long);
                    case ObjectType.UInt64:
                        return typeof(ulong);
                    case ObjectType.Float:
                        return typeof(float);
                    case ObjectType.Double:
                        return typeof(double);
                    case ObjectType.Bool:
                        return typeof(bool);
                }
            }
            if (type == ObjectType.Function)
                return typeof(Delegate);
            return typeof(object);
        }

        public static bool CheckType(ObjectType leftType, ObjectType expected)
        {
            return (leftType & expected) == expected;
        }

        public static bool IsValueType(ObjectType type)
        {
            switch (type)
            {
                case ObjectType.Object:
                    return false;
                case ObjectType.Null:
                    return false;
                case ObjectType.Array:
                case ObjectType.Function:
                case ObjectType.Inbuilt:
                case ObjectType.Void:
                    return false;
                default:
                    return true;
            }
        }
    }
}
