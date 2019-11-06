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

        public static void ToAny(ILGenerator generator, PrimitiveType type)
        {
            if (TypeUtils.IsValueType(type))
                generator.Box(type);
        }

        internal static void ToString(ILGenerator generator, PrimitiveType fromType, string typeName)
        {
            if (TypeUtils.CheckType(fromType, PrimitiveType.String))
                return;
            switch (fromType)
            {
                case PrimitiveType.Undefined:
                    //Push empty string if null
                    generator.Pop();
                    generator.LoadString(string.Empty);
                    break;
                case PrimitiveType.Bool:
                    var elseClause = generator.CreateLabel();
                    var endOfIf = generator.CreateLabel();
                    generator.BranchIfFalse(elseClause);
                    generator.LoadString(bool.TrueString);
                    generator.Branch(endOfIf);
                    generator.DefineLabelPosition(elseClause);
                    generator.LoadString(bool.FalseString);
                    generator.DefineLabelPosition(endOfIf);
                    break;
                case PrimitiveType.Byte:
                case PrimitiveType.UByte:
                case PrimitiveType.Int16:
                case PrimitiveType.UInt16:
                case PrimitiveType.Int32:
                case PrimitiveType.UInt32:
                case PrimitiveType.Int64:
                case PrimitiveType.UInt64:
                case PrimitiveType.Float:
                case PrimitiveType.Double:
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

        internal static void Convert(ILGenerator generator, PrimitiveType fromType, PrimitiveType toType, MethodOptimizationInfo info)
        {
            switch (toType)
            {
                case PrimitiveType.Double:
                    ToNumber(generator, fromType);
                    break;

            }
        }

        private static void ToNumber(ILGenerator generator, PrimitiveType fromType)
        {
            switch (fromType)
            {
                case PrimitiveType.Byte:
                    generator.ConvertToDouble();
                    break;

            }
        }

        internal static void ToPrimitive(ILGenerator generator, PrimitiveType fromType)
        {
            switch (fromType)
            {
                case PrimitiveType.Double:
                    generator.ConvertToDouble();
                    break;
                case PrimitiveType.Float:
                    generator.ConvertToSingle();
                    break;
                case PrimitiveType.UInt64:
                    generator.ConvertToUnsignedInt64();
                    break;
                case PrimitiveType.Int32:
                    generator.ConvertToInt32();
                    break;
                case PrimitiveType.UInt32:
                    generator.ConvertToUnsignedInt32();
                    break;
                case PrimitiveType.Char:
                    generator.ConvertToChar();
                    break;
                case PrimitiveType.Int16:
                    generator.ConvertToInt16();
                    break;
                case PrimitiveType.UInt16:
                    generator.ConvertToUnsignedInt16();
                    break;
                case PrimitiveType.Byte:
                    generator.ConvertToByte();
                    break;
                case PrimitiveType.UByte:
                    generator.ConvertToUnsignedByte();
                    break;
                case PrimitiveType.String:
                case PrimitiveType.Any:
                    throw new InvalidCastException("cannot convert to number");
                default:
                    throw new System.InvalidOperationException("type not found");
            }
        }
    }
}
