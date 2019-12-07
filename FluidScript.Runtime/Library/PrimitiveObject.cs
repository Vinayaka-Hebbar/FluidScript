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
        private readonly object store;
        private readonly RuntimeType Type;

        public PrimitiveObject(object value, RuntimeType type)
        {
            store = value;
            Type = type;
        }

        internal PrimitiveObject(object value, System.Type type)
        {
            store = value;
            Type = Compiler.Emit.TypeUtils.ToPrimitive(type);
        }

        public PrimitiveObject(PrimitiveObject[] value)
        {
            store = value;
            Type = RuntimeType.Array;
        }

        public PrimitiveObject(double value)
        {
            store = value;
            Type = RuntimeType.Double;
        }

        public PrimitiveObject(float value)
        {
            store = value;
            Type = RuntimeType.Float;
        }

        public PrimitiveObject(int value)
        {
            store = value;
            Type = RuntimeType.Int32;
        }

        public PrimitiveObject(char value)
        {
            store = value;
            Type = RuntimeType.Char;
        }

        public PrimitiveObject(bool value)
        {
            store = value;
            Type = RuntimeType.Bool;
        }

        public override string ToString()
        {
            return store.ToString();
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
            return System.Convert.ToDouble(store);
        }

        public override float ToFloat()
        {
            return System.Convert.ToSingle(store);
        }

        public override int ToInt32()
        {
            return System.Convert.ToInt32(store);
        }

        public override char ToChar()
        {
            return System.Convert.ToChar(store);
        }

        public override bool ToBool()
        {
            if ((Type) == RuntimeType.Bool)
                return System.Convert.ToBoolean(store);
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
                    return System.Convert.ToDouble(store);
                case RuntimeType.Float:
                    return System.Convert.ToSingle(store);
                case RuntimeType.Int64:
                    return System.Convert.ToInt64(store);
                case RuntimeType.UInt64:
                    return System.Convert.ToUInt64(store);
                case RuntimeType.Int32:
                    return System.Convert.ToInt32(store);
                case RuntimeType.UInt32:
                    return System.Convert.ToUInt32(store);
                case RuntimeType.Int16:
                    return System.Convert.ToInt16(store);
                case RuntimeType.UInt16:
                    return System.Convert.ToUInt16(store);
                case RuntimeType.Char:
                    return System.Convert.ToChar(store);
                case RuntimeType.Byte:
                    return System.Convert.ToSByte(store);
                case RuntimeType.UByte:
                    return System.Convert.ToByte(store);
                case RuntimeType.Bool:
                    return (bool)store ? 1 : 0;
                case RuntimeType.String:
                    double.TryParse(store.ToString(), out double value);
                    return value;
                case RuntimeType.Any:
                    return System.Convert.ToDouble(store);
                default:
                case RuntimeType.Undefined:
                    return double.NaN;
            }
        }

        public override object ToAny()
        {
            return store;
        }

        public PrimitiveObject[] ToArray()
        {
            if ((Type & RuntimeType.Array) == RuntimeType.Array)
            {
                return (PrimitiveObject[])store;
                //force convert
            }
            return new PrimitiveObject[0];
        }

        public bool IsTypeOf<TSource>()
        {
            return store != null && store.GetType() == typeof(TSource);
        }

        public override System.Type DeclaredType => store.GetType();

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return store == null;
            }
            if (obj is PrimitiveObject primitive)
            {
                return ToNumber().Equals(primitive.ToNumber());
            }
            return store.Equals(obj);
        }

        public bool Equals(PrimitiveObject other)
        {
            return store != null && store.Equals(other.store);
        }

        public override int GetHashCode()
        {
            return store.GetHashCode();
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
            return System.Convert.ToBoolean(store, provider);
        }

        char System.IConvertible.ToChar(System.IFormatProvider provider)
        {
            return System.Convert.ToChar(store, provider);
        }

        sbyte System.IConvertible.ToSByte(System.IFormatProvider provider)
        {
            return System.Convert.ToSByte(store, provider);
        }

        byte System.IConvertible.ToByte(System.IFormatProvider provider)
        {
            return System.Convert.ToByte(store, provider);
        }

        short System.IConvertible.ToInt16(System.IFormatProvider provider)
        {
            return System.Convert.ToInt16(store, provider);
        }

        ushort System.IConvertible.ToUInt16(System.IFormatProvider provider)
        {
            return System.Convert.ToUInt16(store, provider);
        }

        int System.IConvertible.ToInt32(System.IFormatProvider provider)
        {
            return System.Convert.ToInt32(store, provider);
        }

        uint System.IConvertible.ToUInt32(System.IFormatProvider provider)
        {
            return System.Convert.ToUInt32(store, provider);
        }

        long System.IConvertible.ToInt64(System.IFormatProvider provider)
        {
            return System.Convert.ToInt64(store, provider);
        }

        ulong System.IConvertible.ToUInt64(System.IFormatProvider provider)
        {
            return System.Convert.ToUInt64(store, provider);
        }

        float System.IConvertible.ToSingle(System.IFormatProvider provider)
        {
            return System.Convert.ToSingle(store, provider);
        }

        double System.IConvertible.ToDouble(System.IFormatProvider provider)
        {
            return System.Convert.ToDouble(store, provider);
        }

        decimal System.IConvertible.ToDecimal(System.IFormatProvider provider)
        {
            return System.Convert.ToDecimal(store, provider);
        }

        System.DateTime System.IConvertible.ToDateTime(System.IFormatProvider provider)
        {
            return System.DateTime.Now;
        }

        string System.IConvertible.ToString(System.IFormatProvider provider)
        {
            return store.ToString();
        }

        object System.IConvertible.ToType(System.Type conversionType, System.IFormatProvider provider)
        {
            return store;
        }
        #endregion

        public override RuntimeType ReflectedType => Type;


        public static explicit operator int(PrimitiveObject result)
        {
            return (int)result.store;
        }

        public static implicit operator float(PrimitiveObject result)
        {
            return (float)result.store;
        }

        public static implicit operator double(PrimitiveObject result)
        {
            return (double)result.store;
        }

        public static implicit operator string(PrimitiveObject result)
        {
            return result.store.ToString();
        }

        public static implicit operator char(PrimitiveObject result)
        {
            return (char)result.store;
        }

        public static implicit operator bool(PrimitiveObject result)
        {
            return (bool)result.store;
        }
    }
}
