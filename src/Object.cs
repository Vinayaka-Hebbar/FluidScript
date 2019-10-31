using FluidScript.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace FluidScript
{
    [System.Serializable]
    [ComVisible(true)]
    [DefaultMember("Raw")]
    public class Object : System.IConvertible, ISerializable, System.IEquatable<Object>, IRuntimeObject
    {

        public static readonly Object Zero = new Object(0, PrimitiveType.Double);
        public static readonly Object True = new Object(true, PrimitiveType.Bool);
        public static readonly Object False = new Object(false, PrimitiveType.Bool);
        public static readonly Object NaN = new Object(double.NaN, PrimitiveType.Double);
        private static readonly object Any = new object();
        public static readonly Object Void = new Object(Any, PrimitiveType.Any);
        public static readonly Object Null = new Object(Any, PrimitiveType.Null);
        public static readonly Object Empty = new Object(string.Empty, PrimitiveType.String);

        public readonly object Raw;
        public readonly PrimitiveType Type;

        internal static readonly IDictionary<string, PrimitiveType> PrimitiveTypes;
        internal static readonly IDictionary<PrimitiveType, string> Types;

        static Object()
        {
            PrimitiveTypes = new Dictionary<string, PrimitiveType>
            {
                {"byte", PrimitiveType.UByte },
                {"sbyte", PrimitiveType.Byte },
                {"short", PrimitiveType.Int16 },
                {"ushort", PrimitiveType.UInt16 },
                {"int", PrimitiveType.Int32 },
                {"uint",PrimitiveType.UInt32 },
                {"long", PrimitiveType.Int64 },
                {"ulong", PrimitiveType.UInt64 },
                {"float", PrimitiveType.Float },
                {"double", PrimitiveType.Double },
                {"bool", PrimitiveType.Bool },
                {"string", PrimitiveType.String },
                {"char", PrimitiveType.Char },
            };
            Types = new Dictionary<PrimitiveType, string>
            {
                {PrimitiveType.UByte, typeof(byte).FullName },
                { PrimitiveType.Byte, typeof(sbyte).FullName },
                { PrimitiveType.Int16 , typeof(short).FullName},
                { PrimitiveType.UInt16, typeof(ushort).FullName },
                { PrimitiveType.Int32 , typeof(int).FullName},
                {PrimitiveType.UInt32, typeof(uint).FullName },
                {PrimitiveType.Int64 , typeof(long).FullName},
                { PrimitiveType.UInt64, typeof(ulong).FullName },
                { PrimitiveType.Float , typeof(float).FullName},
                { PrimitiveType.Double , typeof(double).FullName},
                { PrimitiveType.Bool , typeof(bool).FullName},
                { PrimitiveType.String , typeof(string).FullName},
                { PrimitiveType.Char , typeof(char).FullName}
            };
        }

        internal Object(object value, PrimitiveType type)
        {
            Raw = value;
            Type = type;
        }

        public string GetTypeName()
        {
            if (Types.ContainsKey(Type))
                return Types[Type];
            return Raw.GetType().FullName;
        }

        internal Object(SerializationInfo info, StreamingContext context)
        {
            Type = (PrimitiveType)info.GetInt32("type");
            Raw = info.GetValue("value", GetType(Type));
        }

        public Object(object value)
        {
            Raw = value;
            Type = GetType(value);
        }

        public Object(object[] value)
        {
            Raw = value;
            Type = PrimitiveType.Array;
        }

        public Object(Object[] value)
        {
            Raw = value;
            Type = PrimitiveType.Array;
        }

        public Object(double value)
        {
            Raw = value;
            Type = PrimitiveType.Double;
        }

        public Object(float value)
        {
            Raw = value;
            Type = PrimitiveType.Float;
        }

        public Object(int value)
        {
            Raw = value;
            Type = PrimitiveType.Int32;
        }

        public Object(char value)
        {
            Raw = value;
            Type = PrimitiveType.Char;
        }

        public Object(string value)
        {
            Raw = value;
            Type = PrimitiveType.String;
        }

        public Object(bool value)
        {
            Raw = value;
            Type = PrimitiveType.Bool;
        }

        public override string ToString()
        {
            return Raw.ToString();
        }

        public double ToDouble()
        {
            if ((Type & PrimitiveType.Double) == PrimitiveType.Double)
                return (double)Raw;
            if ((Type & PrimitiveType.Number) == PrimitiveType.Number)
                return System.Convert.ToDouble(Raw);
            return double.NaN;
        }

        public float ToFloat()
        {
            if ((Type & PrimitiveType.Float) == PrimitiveType.Float)
                return (float)Raw;
            if ((Type & PrimitiveType.Number) == PrimitiveType.Number)
                return System.Convert.ToSingle(Raw);
            return float.NaN;
        }

        public int ToInt32()
        {
            if ((Type & PrimitiveType.Int32) == PrimitiveType.Int32)
                return (int)Raw;
            if ((Type & PrimitiveType.Number) == PrimitiveType.Number)
                return System.Convert.ToInt32(Raw);
            return 0;
        }

        public char ToChar()
        {
            if ((Type & PrimitiveType.Char) == PrimitiveType.Char)
                return (char)Raw;
            return char.MinValue;
        }

        public bool ToBool()
        {
            if ((Type & PrimitiveType.Bool) == PrimitiveType.Bool)
                return (bool)Raw;
            return false;
        }

        public bool IsTypeOf<TSource>()
        {
            return Raw != null && Raw.GetType() == typeof(TSource);
        }

        public double ToNumber()
        {
            //Check Cast
            if ((Type & PrimitiveType.Double) == PrimitiveType.Double)
                return (double)Raw;
            if ((Type & PrimitiveType.Float) == PrimitiveType.Float)
                return (float)Raw;
            if ((Type & PrimitiveType.Int32) == PrimitiveType.Int32)
                return (int)Raw;
            if ((Type & PrimitiveType.Bool) == PrimitiveType.Bool)
                return (bool)Raw ? 1 : 0;
            //force convert
            return System.Convert.ToDouble(Raw);
        }

        public object[] ToArray()
        {
            if ((Type & PrimitiveType.Array) == PrimitiveType.Array)
            {
                //Check Cast
                if ((Type ^ PrimitiveType.Array) == PrimitiveType.Object)
                    return ((Object[])Raw).Select(obj => obj.Raw).ToArray();
                return (object[])Raw;
                //force convert
            }
            return new object[] { };
        }

        public System.Type ToType()
        {
            return Raw.GetType();
        }

        public static System.Type GetType(PrimitiveType type)
        {
            if ((type & PrimitiveType.Int32) == PrimitiveType.Int32)
                return typeof(int);
            if ((type & PrimitiveType.Float) == PrimitiveType.Float)
                return typeof(float);
            if ((type & PrimitiveType.Double) == PrimitiveType.Double)
                return typeof(double);
            if ((type & PrimitiveType.Char) == PrimitiveType.Char)
                return typeof(char);
            if ((type & PrimitiveType.String) == PrimitiveType.String)
                return typeof(string);
            if ((type & PrimitiveType.Bool) == PrimitiveType.Bool)
                return typeof(bool);
            return typeof(object);
        }

        public bool IsNumber() => (Type & PrimitiveType.Number) == PrimitiveType.Number;

        public bool IsString() => (Type & PrimitiveType.String) == PrimitiveType.String;

        public bool IsBool() => (Type & PrimitiveType.Bool) == PrimitiveType.Bool;

        public bool IsChar() => (Type & PrimitiveType.Char) == PrimitiveType.Char;

        public bool IsArray() => (Type & PrimitiveType.Array) == PrimitiveType.Array;

        public bool IsNull
        {
            get
            {
                if ((Type & PrimitiveType.Number) == PrimitiveType.Number)
                {
                    if ((Type & PrimitiveType.Double) == PrimitiveType.Double)
                        return double.IsNaN((double)Raw);
                    if ((Type & PrimitiveType.Float) == PrimitiveType.Float)
                        return float.IsNaN((float)Raw);
                }
                return Raw == null;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return Raw == null;
            }
            if (obj is Object result)
            {
                return Raw != null && Raw.Equals(result.Raw);
            }
            return Raw != null && Raw.Equals(obj);
        }



        public bool Equals(Object other)
        {
            return Raw != null && Raw.Equals(other.Raw);
        }

        public override int GetHashCode()
        {
            return Raw.GetHashCode();
        }

        public static Object operator +(Object result1, Object result2)
        {
            if (result1.IsNumber())
                return new Object(result1.ToNumber() + result2.ToNumber());
            if (result1.IsString())
                return new Object(result1.Raw.ToString() + result2.Raw.ToString());
            return Zero;
        }

        public static Object operator -(Object result1, Object result2)
        {
            return new Object(result1.ToNumber() - result2.ToNumber());
        }

        public static Object operator *(Object result1, Object result2)
        {
            return new Object(result1.ToNumber() * result2.ToNumber());
        }

        public static Object operator /(Object result1, Object result2)
        {
            return new Object(result1.ToNumber() / result2.ToNumber());
        }

        public static Object operator %(Object result1, Object result2)
        {
            return new Object(result1.ToNumber() % result2.ToNumber());
        }

        public static Object operator &(Object result1, Object result2)
        {
            return new Object((int)result1.Raw & (int)result2.Raw);
        }

        public static Object operator |(Object result1, Object result2)
        {
            return new Object((int)result1.Raw | (int)result2.Raw);
        }

        public static Object operator ^(Object result1, Object result2)
        {
            return new Object((int)result1.Raw ^ (int)result2.Raw);
        }

        public static Object operator ~(Object result1)
        {
            result1++;
            return new Object((int)result1.Raw);
        }

        public static Object operator ++(Object result1)
        {
            double value = result1.ToNumber();
            value++;
            return new Object(value);
        }

        public static Object operator --(Object result1)
        {
            double value = result1.ToNumber();
            value--;
            return new Object(value);
        }

        public static Object operator >(Object result1, Object result2)
        {
            return new Object(result1.ToNumber() > result2.ToNumber());
        }

        public static Object operator >=(Object result1, Object result2)
        {
            return new Object(result1.ToNumber() >= result2.ToNumber());
        }

        public static Object operator <(Object result1, Object result2)
        {
            return new Object(result1.ToNumber() < result2.ToNumber());
        }

        public static Object operator <=(Object result1, Object result2)
        {
            return new Object(result1.ToNumber() <= result2.ToNumber());
        }

        public static Object operator ==(Object result1, Object result2)
        {
            return new Object(result1.Equals(result2.Raw));
        }

        public static Object operator !=(Object result1, Object result2)
        {
            return new Object(!result1.Equals(result2.Raw));
        }

        public static Object operator !(Object result1)
        {
            if (result1.IsBool())
                return new Object(!result1.ToBool());
            return Zero;
        }

        public static Object operator +(Object result1, int value)
        {
            if (result1.IsNumber())
                return new Object(result1.ToNumber() + value);
            return new Object(value);
        }

        public static Object operator +(Object result1)
        {
            if (result1.IsNumber())
                return new Object(+result1.ToNumber());
            return Zero;
        }

        public static Object operator -(Object result1, int value)
        {
            return new Object(result1.ToNumber() - value);
        }

        public static Object operator -(Object result1)
        {
            if (result1.IsNumber())
                return new Object(-result1.ToNumber());
            return Zero;
        }

        public static Object operator <<(Object result1, int value)
        {
            return new Object((int)result1.Raw << value);
        }

        public static Object operator >>(Object result1, int value)
        {
            return new Object((int)result1.Raw >> value);
        }

        public static explicit operator int(Object result)
        {
            return (int)result.Raw;
        }

        public static explicit operator float(Object result)
        {
            return (float)result.Raw;
        }

        public static explicit operator double(Object result)
        {
            return (double)result.Raw;
        }

        public static explicit operator string(Object result)
        {
            return result.Raw.ToString();
        }

        public static explicit operator char(Object result)
        {
            return (char)result.Raw;
        }

        public static explicit operator bool(Object result)
        {
            return (bool)result.Raw;
        }

        public static implicit operator Object(float value)
        {
            return new Object(value);
        }

        public static implicit operator Object(double value)
        {
            return new Object(value);
        }

        public static implicit operator Object(int value)
        {
            return new Object(value);
        }

        public static implicit operator Object(string value)
        {
            return new Object(value);
        }

        public static implicit operator Object(bool value)
        {
            return new Object(value);
        }

        public static implicit operator Object(char value)
        {
            return new Object(value);
        }

        public static implicit operator Object(Function function)
        {
            return new Object(function);
        }

        public static implicit operator Object(object[] value)
        {
            return new Object(value);
        }

        public static implicit operator Object(Object[] value)
        {
            return new Object(value);
        }

        private static PrimitiveType GetType(object value)
        {
            switch (value)
            {
                case string _:
                    return PrimitiveType.String;
                case double _:
                    return PrimitiveType.Double;
                case float _:
                    return PrimitiveType.Float;
                case int _:
                    return PrimitiveType.Int32;
                case char _:
                    return PrimitiveType.Char;
                case bool _:
                    return PrimitiveType.Bool;
            }
            if (value.GetType().IsArray)
                return PrimitiveType.Array;
            return PrimitiveType.Null;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("value", Raw);
            info.AddValue("type", (int)Type);
        }

        public static Object Attach(Object value, PrimitiveType type)
        {
            return new Object(value.Raw, value.Type | type);
        }


        public static PrimitiveType GetType(string value)
        {
            if (PrimitiveTypes.ContainsKey(value))
                return PrimitiveTypes[value];
            return PrimitiveType.Object;
        }

        #region Convertible
        System.TypeCode System.IConvertible.GetTypeCode()
        {
            switch (Type)
            {
                case PrimitiveType.Int32:
                    return System.TypeCode.Int32;
                case PrimitiveType.Float:
                    return System.TypeCode.Single;
                case PrimitiveType.Double:
                    return System.TypeCode.Double;
                case PrimitiveType.Char:
                    return System.TypeCode.Char;
                case PrimitiveType.String:
                    return System.TypeCode.String;
                case PrimitiveType.Bool:
                    return System.TypeCode.Boolean;
                case PrimitiveType.Null:
                default:
                    if (Raw != null)
                    {
                        return GetOtherType();
                    }
                    return System.TypeCode.Empty;
            }
        }

        private System.TypeCode GetOtherType()
        {
            switch (Raw)
            {
                case sbyte _:
                    return System.TypeCode.SByte;
                case byte _:
                    return System.TypeCode.Byte;
                case short _:
                    return System.TypeCode.Int16;
                case long _:
                    return System.TypeCode.Int64;
                case ushort _:
                    return System.TypeCode.UInt16;
                case uint _:
                    return System.TypeCode.UInt32;
                case ulong _:
                    return System.TypeCode.UInt64;
                case decimal _:
                    return System.TypeCode.Decimal;
                case System.DateTime _:
                    return System.TypeCode.DateTime;
                default:
                    return System.TypeCode.Object;
            }
        }

        bool System.IConvertible.ToBoolean(System.IFormatProvider provider)
        {
            return (bool)Raw;
        }

        char System.IConvertible.ToChar(System.IFormatProvider provider)
        {
            return (char)Raw;
        }

        sbyte System.IConvertible.ToSByte(System.IFormatProvider provider)
        {
            return (sbyte)Raw;
        }

        byte System.IConvertible.ToByte(System.IFormatProvider provider)
        {
            return (byte)Raw;
        }

        short System.IConvertible.ToInt16(System.IFormatProvider provider)
        {
            return (short)Raw;
        }

        ushort System.IConvertible.ToUInt16(System.IFormatProvider provider)
        {
            return (ushort)Raw;
        }

        int System.IConvertible.ToInt32(System.IFormatProvider provider)
        {
            return (int)Raw;
        }

        uint System.IConvertible.ToUInt32(System.IFormatProvider provider)
        {
            return (uint)Raw;
        }

        long System.IConvertible.ToInt64(System.IFormatProvider provider)
        {
            return (long)Raw;
        }

        ulong System.IConvertible.ToUInt64(System.IFormatProvider provider)
        {
            return (ulong)Raw;
        }

        float System.IConvertible.ToSingle(System.IFormatProvider provider)
        {
            return (float)Raw;
        }

        double System.IConvertible.ToDouble(System.IFormatProvider provider)
        {
            return (double)Raw;
        }

        decimal System.IConvertible.ToDecimal(System.IFormatProvider provider)
        {
            return (decimal)Raw;
        }

        System.DateTime System.IConvertible.ToDateTime(System.IFormatProvider provider)
        {
            return (System.DateTime)Raw;
        }

        string System.IConvertible.ToString(System.IFormatProvider provider)
        {
            return Raw.ToString();
        }

        object System.IConvertible.ToType(System.Type conversionType, System.IFormatProvider provider)
        {
            return Raw;
        }

        #endregion
    }
}
