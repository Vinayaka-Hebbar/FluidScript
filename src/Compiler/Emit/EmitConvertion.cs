using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluidScript.Compiler.Emit
{
    public static class EmitConvertion
    {
        public static void ToAny(ILGenerator generator, Type type)
        {
            if (type.IsValueType)
                generator.Box(type);
        }

        public static void ToAny(ILGenerator generator, ObjectType type)
        {
            if (TypeUtils.IsValueType(type))
                generator.Box(type);
        }

        internal static void ToString(ILGenerator generator, ObjectType fromType, string typeName)
        {
            if (TypeUtils.CheckType(fromType, ObjectType.String))
                return;
            switch (fromType)
            {
                case ObjectType.Null:
                    //Push empty string if null
                    generator.Pop();
                    generator.LoadString(string.Empty);
                    break;
                case ObjectType.Bool:
                    var elseClause = generator.CreateLabel();
                    var endOfIf = generator.CreateLabel();
                    generator.BranchIfFalse(elseClause);
                    generator.LoadString(bool.TrueString);
                    generator.Branch(endOfIf);
                    generator.DefineLabelPosition(elseClause);
                    generator.LoadString(bool.FalseString);
                    generator.DefineLabelPosition(endOfIf);
                    break;
                case ObjectType.Byte:
                case ObjectType.UByte:
                case ObjectType.Int16:
                case ObjectType.UInt16:
                case ObjectType.Int32:
                case ObjectType.UInt32:
                case ObjectType.Int64:
                case ObjectType.UInt64:
                case ObjectType.Float:
                case ObjectType.Double:
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

        internal static void ToNumber(ILGenerator generator, ObjectType fromType)
        {
            switch (fromType)
            {
                case ObjectType.Byte:
                case ObjectType.UByte:
                case ObjectType.UInt16:
                case ObjectType.Int16:
                case ObjectType.Int32:
                    generator.ConvertToInt32();
                    break;
                case ObjectType.UInt32:
                    generator.ConvertToUnsignedInt32();
                    break;
                case ObjectType.Double:
                    generator.ConvertToDouble();
                    break;
                case ObjectType.Float:
                    generator.ConvertToSingle();
                    break;
                case ObjectType.String:
                case ObjectType.Object:
                    throw new InvalidCastException("cannot convert to number");
                default:
                    throw new System.InvalidOperationException("type not found");
            }
        }
    }
}
