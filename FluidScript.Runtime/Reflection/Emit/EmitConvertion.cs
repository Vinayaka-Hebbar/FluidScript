
namespace FluidScript.Reflection.Emit
{
    public static class EmitConvertion
    {
        public static void ToAny(ILGenerator generator, System.Type type)
        {
            if (type.IsValueType)
                generator.Box(type);
        }

        internal static void Convert(ILGenerator generator, RuntimeType fromType, RuntimeType toType)
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
                case RuntimeType.Int32:
                    generator.ConvertToInt32();
                    break;
                case RuntimeType.Char:
                    generator.ConvertToChar();
                    break;
                case RuntimeType.Int16:
                    generator.ConvertToInt16();
                    break;
                case RuntimeType.Byte:
                    generator.ConvertToByte();
                    break;
                case RuntimeType.String:
                case RuntimeType.Any:
                    throw new System.InvalidCastException("cannot convert to number");
                default:
                    throw new System.InvalidOperationException("type not found");
            }
        }

        internal static void Convert(MethodBodyGenerator generator, System.Type from, System.Type to)
        {
            var method = to.GetMethod("op_Implicit", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static, null, new System.Type[1] { from }, null);
            if (method != null && method.ReturnType == to)
                generator.Call(method);
            method = from.GetMethod("op_Implicit", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static, null, new System.Type[1] { from }, null);
            if (method != null && method.ReturnType == to)
                generator.Call(method);
        }
    }
}
