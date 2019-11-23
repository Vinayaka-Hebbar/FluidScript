namespace FluidScript.Library
{
    /// <summary>
    /// Represents Primitive Type Object
    /// </summary>
    public sealed class PrimitiveObject : RuntimeObject, System.IConvertible
    {
        public static readonly RuntimeObject Zero = new PrimitiveObject(0, RuntimeType.Double);
        public static readonly RuntimeObject True = new PrimitiveObject(true, RuntimeType.Bool);
        public static readonly RuntimeObject False = new PrimitiveObject(false, RuntimeType.Bool);
        public static readonly RuntimeObject NaN = new PrimitiveObject(double.NaN, RuntimeType.Double);

        private static Compiler.Metadata.Prototype prototype;
        private readonly object Store;
        private readonly RuntimeType Type;

        public PrimitiveObject(object value, RuntimeType type)
        {
            Store = value;
            Type = type;
        }

        internal PrimitiveObject(object value, System.Type type)
        {
            Store = value;
            Type = Compiler.Emit.TypeUtils.ToPrimitive(type);
        }

        public PrimitiveObject(PrimitiveObject[] value)
        {
            Store = value;
            Type = RuntimeType.Array;
        }

        public PrimitiveObject(double value)
        {
            Store = value;
            Type = RuntimeType.Double;
        }

        public PrimitiveObject(float value)
        {
            Store = value;
            Type = RuntimeType.Float;
        }

        public PrimitiveObject(int value)
        {
            Store = value;
            Type = RuntimeType.Int32;
        }

        public PrimitiveObject(char value)
        {
            Store = value;
            Type = RuntimeType.Char;
        }

        public PrimitiveObject(bool value)
        {
            Store = value;
            Type = RuntimeType.Bool;
        }

        public override string ToString()
        {
            return Store.ToString();
        }

        public override bool IsNumber() => (Type & RuntimeType.Double) == RuntimeType.Double;

        public override bool IsString() => (Type & RuntimeType.String) == RuntimeType.String;

        public override bool IsBool() => (Type & RuntimeType.Bool) == RuntimeType.Bool;

        public override bool IsChar() => (Type & RuntimeType.Char) == RuntimeType.Char;

        public override bool IsArray() => (Type & RuntimeType.Array) == RuntimeType.Array;

        public override bool IsNull()
        {
            if ((Type & RuntimeType.Double) == RuntimeType.Double)
            {
                //todo for Nan
                return false;
            }
            return true;
        }

        public override double ToDouble()
        {
            return System.Convert.ToDouble(Store);
        }

        public override float ToFloat()
        {
            return System.Convert.ToSingle(Store);
        }

        public override int ToInt32()
        {
            return System.Convert.ToInt32(Store);
        }

        public override char ToChar()
        {
            return System.Convert.ToChar(Store);
        }

        public override bool ToBool()
        {
            if ((Type) == RuntimeType.Bool)
                return System.Convert.ToBoolean(Store);
            return Type != RuntimeType.Undefined;
        }

        /// <summary>
        /// Converts to Number
        /// </summary>
        /// <returns></returns>
        public override double ToNumber()
        {
            //Check Cast
            switch (Type)
            {
                case RuntimeType.Double:
                    return System.Convert.ToDouble(Store);
                case RuntimeType.Float:
                    return System.Convert.ToSingle(Store);
                case RuntimeType.Int64:
                    return System.Convert.ToInt64(Store);
                case RuntimeType.UInt64:
                    return System.Convert.ToUInt64(Store);
                case RuntimeType.Int32:
                    return System.Convert.ToInt32(Store);
                case RuntimeType.UInt32:
                    return System.Convert.ToUInt32(Store);
                case RuntimeType.Int16:
                    return System.Convert.ToInt16(Store);
                case RuntimeType.UInt16:
                    return System.Convert.ToUInt16(Store);
                case RuntimeType.Char:
                    return System.Convert.ToChar(Store);
                case RuntimeType.Byte:
                    return System.Convert.ToSByte(Store);
                case RuntimeType.UByte:
                    return System.Convert.ToByte(Store);
                case RuntimeType.Bool:
                    return (bool)Store ? 1 : 0;
                case RuntimeType.String:
                    double.TryParse(Store.ToString(), out double value);
                    return value;
                case RuntimeType.Any:
                    return System.Convert.ToDouble(Store);
                default:
                case RuntimeType.Undefined:
                    return double.NaN;
            }
        }

        public PrimitiveObject[] ToArray()
        {
            if ((Type & RuntimeType.Array) == RuntimeType.Array)
            {
                return (PrimitiveObject[])Store;
                //force convert
            }
            return new PrimitiveObject[0];
        }

        public bool IsTypeOf<TSource>()
        {
            return Store != null && Store.GetType() == typeof(TSource);
        }

        public override System.Type DeclaredType => Store.GetType();

        public override bool Equals(object obj)
        {
            double value = ToNumber();
            if (obj == null)
            {
                return Store == null;
            }
            if (obj is PrimitiveObject result)
            {
                return value.Equals(result.ToNumber());
            }
            return value.Equals(obj);
        }

        public bool Equals(PrimitiveObject other)
        {
            return Store != null && Store.Equals(other.Store);
        }

        public override int GetHashCode()
        {
            return Store.GetHashCode();
        }

        public override Compiler.Metadata.Prototype GetPrototype()
        {
            if (prototype is null)
            {
                prototype = Compiler.Metadata.Prototype.Create(GetType());
                prototype.IsSealed = true;
            }
            return prototype;
        }

        #region IConvertible

        System.TypeCode System.IConvertible.GetTypeCode()
        {
            switch (Type)
            {
                case RuntimeType.Double:
                    return System.TypeCode.Double;
                case RuntimeType.Float:
                    return System.TypeCode.Single;
                case RuntimeType.Int64:
                    return System.TypeCode.Int64;
                case RuntimeType.UInt64:
                    return System.TypeCode.UInt64;
                case RuntimeType.Int32:
                    return System.TypeCode.Int32;
                case RuntimeType.UInt32:
                    return System.TypeCode.UInt32;
                case RuntimeType.Int16:
                    return System.TypeCode.Int16;
                case RuntimeType.UInt16:
                    return System.TypeCode.UInt16;
                case RuntimeType.Char:
                    return System.TypeCode.Char;
                case RuntimeType.Byte:
                    return System.TypeCode.SByte;
                case RuntimeType.UByte:
                    return System.TypeCode.Byte;
                case RuntimeType.Bool:
                    return System.TypeCode.Boolean;
                default:
                    return System.TypeCode.Object;
            }
        }

        bool System.IConvertible.ToBoolean(System.IFormatProvider provider)
        {
            return System.Convert.ToBoolean(Store, provider);
        }

        char System.IConvertible.ToChar(System.IFormatProvider provider)
        {
            return System.Convert.ToChar(Store, provider);
        }

        sbyte System.IConvertible.ToSByte(System.IFormatProvider provider)
        {
            return System.Convert.ToSByte(Store, provider);
        }

        byte System.IConvertible.ToByte(System.IFormatProvider provider)
        {
            return System.Convert.ToByte(Store, provider);
        }

        short System.IConvertible.ToInt16(System.IFormatProvider provider)
        {
            return System.Convert.ToInt16(Store, provider);
        }

        ushort System.IConvertible.ToUInt16(System.IFormatProvider provider)
        {
            return System.Convert.ToUInt16(Store, provider);
        }

        int System.IConvertible.ToInt32(System.IFormatProvider provider)
        {
            return System.Convert.ToInt32(Store, provider);
        }

        uint System.IConvertible.ToUInt32(System.IFormatProvider provider)
        {
            return System.Convert.ToUInt32(Store, provider);
        }

        long System.IConvertible.ToInt64(System.IFormatProvider provider)
        {
            return System.Convert.ToInt64(Store, provider);
        }

        ulong System.IConvertible.ToUInt64(System.IFormatProvider provider)
        {
            return System.Convert.ToUInt64(Store, provider);
        }

        float System.IConvertible.ToSingle(System.IFormatProvider provider)
        {
            return System.Convert.ToSingle(Store, provider);
        }

        double System.IConvertible.ToDouble(System.IFormatProvider provider)
        {
            return System.Convert.ToDouble(Store, provider);
        }

        decimal System.IConvertible.ToDecimal(System.IFormatProvider provider)
        {
            return System.Convert.ToDecimal(Store, provider);
        }

        System.DateTime System.IConvertible.ToDateTime(System.IFormatProvider provider)
        {
            return System.DateTime.Now;
        }

        string System.IConvertible.ToString(System.IFormatProvider provider)
        {
            return Store.ToString();
        }

        object System.IConvertible.ToType(System.Type conversionType, System.IFormatProvider provider)
        {
            return Store;
        }
        #endregion

        public override RuntimeType ReflectedType => Type;


        public static explicit operator int(PrimitiveObject result)
        {
            return (int)result.Store;
        }

        public static implicit operator float(PrimitiveObject result)
        {
            return (float)result.Store;
        }

        public static implicit operator double(PrimitiveObject result)
        {
            return (double)result.Store;
        }

        public static implicit operator string(PrimitiveObject result)
        {
            return result.Store.ToString();
        }

        public static implicit operator char(PrimitiveObject result)
        {
            return (char)result.Store;
        }

        public static implicit operator bool(PrimitiveObject result)
        {
            return (bool)result.Store;
        }
    }
}
