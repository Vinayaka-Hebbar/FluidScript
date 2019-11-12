using System;

namespace FluidScript.Compiler.Emit
{
    public static class EmitConvertion
    {
        public static void ToAny(ILGenerator generator, Type type)
        {
            if (type.IsValueType)
                generator.Box(type);
        }

        public static void ToAny(ILGenerator generator, RuntimeType type)
        {
            if (TypeUtils.IsValueType(type))
                generator.Box(type);
        }

        internal static void ToString(ILGenerator generator, RuntimeType fromType, string typeName)
        {
            if (TypeUtils.CheckType(fromType, RuntimeType.String))
                return;
            switch (fromType)
            {
                case RuntimeType.Undefined:
                    //Push empty string if null
                    generator.Pop();
                    generator.LoadString(string.Empty);
                    break;
                case RuntimeType.Bool:
                    var elseClause = generator.CreateLabel();
                    var endOfIf = generator.CreateLabel();
                    generator.BranchIfFalse(elseClause);
                    generator.LoadString(bool.TrueString);
                    generator.Branch(endOfIf);
                    generator.DefineLabelPosition(elseClause);
                    generator.LoadString(bool.FalseString);
                    generator.DefineLabelPosition(endOfIf);
                    break;
                case RuntimeType.Byte:
                case RuntimeType.UByte:
                case RuntimeType.Int16:
                case RuntimeType.UInt16:
                case RuntimeType.Int32:
                case RuntimeType.UInt32:
                case RuntimeType.Int64:
                case RuntimeType.UInt64:
                case RuntimeType.Float:
                case RuntimeType.Double:
                    if (TypeUtils.IsValueType(fromType))
                        generator.Box(fromType);
                    else
                    {
                        throw new System.InvalidOperationException(string.Format("{0} not found", typeName));
                    }
                    break;
                default:
                    throw new System.InvalidOperationException(string.Format("unsupported type {0}", typeName));
            }
        }

        internal static void Convert(ILGenerator generator, RuntimeType fromType, RuntimeType toType, MethodOptimizationInfo info)
        {
            switch (toType)
            {
                case RuntimeType.Double:
                    ToNumber(generator, fromType);
                    break;

            }
        }

        private static void ToNumber(ILGenerator generator, RuntimeType fromType)
        {
            switch (fromType)
            {
                case RuntimeType.Byte:
                    generator.ConvertToDouble();
                    break;

            }
        }

        internal static void ToPrimitive(ILGenerator generator, RuntimeType fromType)
        {
            switch (fromType)
            {
                case RuntimeType.Double:
                    generator.ConvertToDouble();
                    break;
                case RuntimeType.Float:
                    generator.ConvertToSingle();
                    break;
                case RuntimeType.UInt64:
                    generator.ConvertToUnsignedInt64();
                    break;
                case RuntimeType.Int32:
                    generator.ConvertToInt32();
                    break;
                case RuntimeType.UInt32:
                    generator.ConvertToUnsignedInt32();
                    break;
                case RuntimeType.Char:
                    generator.ConvertToChar();
                    break;
                case RuntimeType.Int16:
                    generator.ConvertToInt16();
                    break;
                case RuntimeType.UInt16:
                    generator.ConvertToUnsignedInt16();
                    break;
                case RuntimeType.Byte:
                    generator.ConvertToByte();
                    break;
                case RuntimeType.UByte:
                    generator.ConvertToUnsignedByte();
                    break;
                case RuntimeType.String:
                case RuntimeType.Any:
                    throw new InvalidCastException("cannot convert to number");
                default:
                    throw new System.InvalidOperationException("type not found");
            }
        }
    }
}
