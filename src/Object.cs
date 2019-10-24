using FluidScript.Compiler;
using System;
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

        public static readonly Object Zero = new Object(0, ObjectType.Double | ObjectType.Inbuilt);
        public static readonly Object True = new Object(true, ObjectType.Bool | ObjectType.Inbuilt);
        public static readonly Object False = new Object(false, ObjectType.Bool | ObjectType.Inbuilt);
        public static readonly Object NaN = new Object(double.NaN, ObjectType.Double | ObjectType.Inbuilt);
        private static readonly object Any = new object();
        public static readonly Object Void = new Object(Any, ObjectType.Void | ObjectType.Inbuilt);
        public static readonly Object Null = new Object(Any, ObjectType.Null | ObjectType.Inbuilt);
        public static readonly Object Empty = new Object(string.Empty, ObjectType.String | ObjectType.Inbuilt);

        public readonly object Raw;
        public readonly ObjectType Type;

        internal static readonly IDictionary<string, ObjectType> PrimitiveTypes;
        internal static readonly IDictionary<ObjectType, string> Types;

        static Object()
        {
            PrimitiveTypes = new Dictionary<string, ObjectType>
            {
                {"byte", ObjectType.UByte },
                {"sbyte", ObjectType.Byte },
                {"short", ObjectType.Int16 },
                {"ushort", ObjectType.UInt16 },
                {"int", ObjectType.Int32 },
                {"uint",ObjectType.UInt32 },
                {"long", ObjectType.Int64 },
                {"ulong", ObjectType.UInt64 },
                {"float", ObjectType.Float },
                {"double", ObjectType.Double },
                {"bool", ObjectType.Bool },
                {"string", ObjectType.String },
                {"char", ObjectType.Char },
            };
            Types = new Dictionary<ObjectType, string>
            {
                {ObjectType.UByte, typeof(byte).FullName },
                { ObjectType.Byte, typeof(sbyte).FullName },
                { ObjectType.Int16 , typeof(short).FullName},
                { ObjectType.UInt16, typeof(ushort).FullName },
                { ObjectType.Int32 , typeof(int).FullName},
                {ObjectType.UInt32, typeof(uint).FullName },
                {ObjectType.Int64 , typeof(long).FullName},
                { ObjectType.UInt64, typeof(ulong).FullName },
                { ObjectType.Float , typeof(float).FullName},
                { ObjectType.Double , typeof(double).FullName},
                { ObjectType.Bool , typeof(bool).FullName},
                { ObjectType.String , typeof(string).FullName},
                { ObjectType.Char , typeof(char).FullName}
            };
        }

        internal Object(object value, ObjectType type)
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
            Type = (ObjectType)info.GetInt32("type");
            Raw = info.GetValue("value", GetType(Type));
        }

        public Object(object value)
        {
            Raw = value;
            Type = GetType(value);
        }

        public Object(IFunction function)
        {
            Raw = function;
            Type = ObjectType.Function;
        }

        public Object(object[] value)
        {
            Raw = value;
            Type = ObjectType.Array;
        }

        public Object(Object[] value)
        {
            Raw = value;
            Type = ObjectType.Array;
        }

        public Object(double value)
        {
            Raw = value;
            Type = ObjectType.Double;
        }

        public Object(float value)
        {
            Raw = value;
            Type = ObjectType.Float;
        }

        public Object(int value)
        {
            Raw = value;
            Type = ObjectType.Int32;
        }

        public Object(char value)
        {
            Raw = value;
            Type = ObjectType.Char;
        }

        public Object(string value)
        {
            Raw = value;
            Type = ObjectType.String;
        }

        public Object(bool value)
        {
            Raw = value;
            Type = ObjectType.Bool;
        }

        public override string ToString()
        {
            return Raw.ToString();
        }

        public double ToDouble()
        {
            if ((Type & ObjectType.Double) == ObjectType.Double)
                return (double)Raw;
            if ((Type & ObjectType.Number) == ObjectType.Number)
                return System.Convert.ToDouble(Raw);
            return double.NaN;
        }

        public float ToFloat()
        {
            if ((Type & ObjectType.Float) == ObjectType.Float)
                return (float)Raw;
            if ((Type & ObjectType.Number) == ObjectType.Number)
                return System.Convert.ToSingle(Raw);
            return float.NaN;
        }

        public int ToInt32()
        {
            if ((Type & ObjectType.Int32) == ObjectType.Int32)
                return (int)Raw;
            if ((Type & ObjectType.Number) == ObjectType.Number)
                return System.Convert.ToInt32(Raw);
            return 0;
        }

        public char ToChar()
        {
            if ((Type & ObjectType.Char) == ObjectType.Char)
                return (char)Raw;
            return char.MinValue;
        }

        public bool ToBool()
        {
            if ((Type & ObjectType.Bool) == ObjectType.Bool)
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
            if ((Type & ObjectType.Double) == ObjectType.Double)
                return (double)Raw;
            if ((Type & ObjectType.Float) == ObjectType.Float)
                return (float)Raw;
            if ((Type & ObjectType.Int32) == ObjectType.Int32)
                return (int)Raw;
            if ((Type & ObjectType.Bool) == ObjectType.Bool)
                return (bool)Raw ? 1 : 0;
            //force convert
            return System.Convert.ToDouble(Raw);
        }

        public object[] ToArray()
        {
            if ((Type & ObjectType.Array) == ObjectType.Array)
            {
                //Check Cast
                if ((Type ^ ObjectType.Array) == ObjectType.Object)
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

        public static System.Type GetType(ObjectType type)
        {
            if ((type & ObjectType.Int32) == ObjectType.Int32)
                return typeof(int);
            if ((type & ObjectType.Float) == ObjectType.Float)
                return typeof(float);
            if ((type & ObjectType.Double) == ObjectType.Double)
                return typeof(double);
            if ((type & ObjectType.Char) == ObjectType.Char)
                return typeof(char);
            if ((type & ObjectType.String) == ObjectType.String)
                return typeof(string);
            if ((type & ObjectType.Bool) == ObjectType.Bool)
                return typeof(bool);
            return typeof(object);
        }

        public bool IsNumber() => (Type & ObjectType.Number) == ObjectType.Number;

        public bool IsString() => (Type & ObjectType.String) == ObjectType.String;

        public bool IsBool() => (Type & ObjectType.Bool) == ObjectType.Bool;

        public bool IsInbuilt() => (Type & ObjectType.Inbuilt) == ObjectType.Inbuilt;

        public bool IsChar() => (Type & ObjectType.Char) == ObjectType.Char;

        public bool IsArray() => (Type & ObjectType.Array) == ObjectType.Array;

        public bool IsNull
        {
            get
            {
                if ((Type & ObjectType.Number) == ObjectType.Number)
                {
                    if ((Type & ObjectType.Double) == ObjectType.Double)
                        return double.IsNaN((double)Raw);
                    if ((Type & ObjectType.Float) == ObjectType.Float)
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

        private static ObjectType GetType(object value)
        {
            switch (value)
            {
                case string _:
                    return ObjectType.String;
                case double _:
                    return ObjectType.Double;
                case float _:
                    return ObjectType.Float;
                case int _:
                    return ObjectType.Int32;
                case char _:
                    return ObjectType.Char;
                case bool _:
                    return ObjectType.Bool;
                case System.Action _:
                    return ObjectType.Function;
            }
            if (value.GetType().IsArray)
                return ObjectType.Array;
            return ObjectType.Null;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("value", Raw);
            info.AddValue("type", (int)Type);
        }

        public static Object Attach(Object value, ObjectType type)
        {
            return new Object(value.Raw, value.Type | type);
        }


        public static ObjectType GetType(string value)
        {
            if (PrimitiveTypes.ContainsKey(value))
                return PrimitiveTypes[value];
            return ObjectType.Object;
        }

        #region Convertible
        System.TypeCode System.IConvertible.GetTypeCode()
        {
            switch (Type)
            {
                case ObjectType.Int32:
                    return System.TypeCode.Int32;
                case ObjectType.Float:
                    return System.TypeCode.Single;
                case ObjectType.Double:
                    return System.TypeCode.Double;
                case ObjectType.Char:
                    return System.TypeCode.Char;
                case ObjectType.String:
                    return System.TypeCode.String;
                case ObjectType.Bool:
                    return System.TypeCode.Boolean;
                case ObjectType.Null:
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
