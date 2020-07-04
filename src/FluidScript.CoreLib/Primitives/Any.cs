﻿using FluidScript.Runtime;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace FluidScript
{
    public struct Any : IConvertible
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        object m_value;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        Type m_type;

        public Any(object value)
        {
            m_value = value;
            m_type = null;
        }

        private Any(object value, Type type)
        {
            m_value = value;
            m_type = type;
        }

        public Type Type
        {
            get
            {
                if (m_type == null)
                {
                    if (m_value is null)
                        m_type = typeof(object);
                    m_type = m_value.GetType();
                }
                return m_type;
            }
        }

        public Any this[Any key]
        {
            get
            {
                object[] args = new object[] { key.m_value };
                var indexer = TypeUtils.FindGetIndexer(Type, args, out ArgumentConversions conversions);
                if (indexer != null)
                {
                    conversions.Invoke(ref args);
                    return new Any(indexer.Invoke(m_value, args));
                }
                return default(Any);
            }
            set
            {
                object[] args = new object[] { key.m_value, value.m_value };
                var indexer = TypeUtils.FindSetIndexer(Type, args, out ArgumentConversions conversions);
                if (indexer != null)
                {
                    conversions.Invoke(ref args);
                    indexer.Invoke(m_value, args);
                }
            }
        }

        public Any this[Any[] keys]
        {
            get
            {
                object[] args = GetArgs(keys);
                var indexer = TypeUtils.FindGetIndexer(Type, args, out ArgumentConversions conversions);
                if (indexer != null)
                {
                    conversions.Invoke(ref args);
                    return new Any(indexer.Invoke(m_value, args));
                }
                return default(Any);
            }
            set
            {
                object[] args = new object[keys.Length + 1];
                int i;
                for (i = 0; i < keys.Length; i++)
                {
                    args[i] = keys[i].m_value;
                }
                args[i] = value.m_value;
                var indexer = TypeUtils.FindSetIndexer(Type, args, out ArgumentConversions conversions);
                if (indexer != null)
                {
                    conversions.Invoke(ref args);
                    indexer.Invoke(m_value, args);
                }
            }
        }

        public override string ToString()
        {
            if (m_value is null)
                return string.Empty;
            return m_value.ToString();
        }

        public override bool Equals(object obj)
        {
            if (obj is Any other)
            {
                if (other.m_value is null)
                    return m_value is null;
                return m_value.Equals(other.m_value);
            }
            return false;
        }

        public Any Call(string name)
        {
            var args = new object[0];
            if (TypeUtils.TryFindMethod(Type, name, args, out System.Reflection.MethodInfo method, out ArgumentConversions conversions))
            {
                conversions.Invoke(ref args);
                return op_Implicit(method.Invoke(m_value, args));
            }
            return default(Any);
        }

        public Any Call(string name, params Any[] args)
        {
            var actualArgs = GetArgs(args);
            if (TypeUtils.TryFindMethod(Type, name, actualArgs, out System.Reflection.MethodInfo method, out ArgumentConversions conversions))
            {
                conversions.Invoke(ref actualArgs);
                return new Any(method.Invoke(m_value, actualArgs));
            }
            return default(Any);
        }

        public override int GetHashCode()
        {
            if (m_value is null)
                return 0;
            return m_value.GetHashCode();
        }

        static object[] GetArgs(Any[] keys)
        {
            object[] args = new object[keys.Length];
            for (int i = 0; i < keys.Length; i++)
            {
                args[i] = keys[i].m_value;
            }

            return args;
        }

        #region IConvertible
        public TypeCode GetTypeCode()
        {
            if (m_value is IConvertible)
                return ((IConvertible)m_value).GetTypeCode();
            return TypeCode.Object;
        }

        bool IConvertible.ToBoolean(IFormatProvider provider)
        {
            if (m_value is IConvertible)
                return ((IConvertible)m_value).ToBoolean(provider);
            return false;
        }

        byte IConvertible.ToByte(IFormatProvider provider)
        {
            if (m_value is IConvertible)
                return ((IConvertible)m_value).ToByte(provider);
            return 0;
        }

        char IConvertible.ToChar(IFormatProvider provider)
        {
            if (m_value is IConvertible)
                return ((IConvertible)m_value).ToChar(provider);
            return char.MinValue;
        }

        DateTime IConvertible.ToDateTime(IFormatProvider provider)
        {
            if (m_value is IConvertible)
                return ((IConvertible)m_value).ToDateTime(provider);
            return default(DateTime);
        }

        decimal IConvertible.ToDecimal(IFormatProvider provider)
        {
            if (m_value is IConvertible)
                return ((IConvertible)m_value).ToDecimal(provider);
            return 0;
        }

        double IConvertible.ToDouble(IFormatProvider provider)
        {
            if (m_value is IConvertible)
                return ((IConvertible)m_value).ToDouble(provider);
            return 0;
        }

        short IConvertible.ToInt16(IFormatProvider provider)
        {
            if (m_value is IConvertible)
                return ((IConvertible)m_value).ToInt16(provider);
            return 0;
        }

        int IConvertible.ToInt32(IFormatProvider provider)
        {
            if (m_value is IConvertible)
                return ((IConvertible)m_value).ToInt32(provider);
            return 0;
        }

        long IConvertible.ToInt64(IFormatProvider provider)
        {
            if (m_value is IConvertible)
                return ((IConvertible)m_value).ToInt64(provider);
            return 0;
        }

        sbyte IConvertible.ToSByte(IFormatProvider provider)
        {
            if (m_value is IConvertible)
                return ((IConvertible)m_value).ToSByte(provider);
            return 0;
        }

        float IConvertible.ToSingle(IFormatProvider provider)
        {
            if (m_value is IConvertible)
                return ((IConvertible)m_value).ToSingle(provider);
            return 0;
        }

        string IConvertible.ToString(IFormatProvider provider)
        {
            if (m_value is IConvertible)
                return ((IConvertible)m_value).ToString(provider);
            return ToString();
        }

        object IConvertible.ToType(Type conversionType, IFormatProvider provider)
        {
            if (m_value is null)
                return null;
            var valueType = m_value.GetType();
            if (TypeUtils.AreReferenceAssignable(conversionType, valueType))
                return m_value;
            if (TypeUtils.TryImplicitConvert(valueType, conversionType, out System.Reflection.MethodInfo conversion))
                return conversion.Invoke(null, new object[] { m_value });
            throw new InvalidCastException($"Unable to cast object of type {valueType} to {conversionType}");
        }

        ushort IConvertible.ToUInt16(IFormatProvider provider)
        {
            if (m_value is IConvertible)
                return ((IConvertible)m_value).ToUInt16(provider);
            return 0;
        }

        uint IConvertible.ToUInt32(IFormatProvider provider)
        {
            if (m_value is IConvertible)
                return ((IConvertible)m_value).ToUInt32(provider);
            return 0;
        }

        ulong IConvertible.ToUInt64(IFormatProvider provider)
        {
            if (m_value is IConvertible)
                return ((IConvertible)m_value).ToUInt64(provider);
            return 0;
        }
        #endregion

        public static implicit operator Boolean(Any value)
        {
            if (value.m_value is Boolean)
                return (Boolean)value.m_value;
            if (value.m_value is bool)
                return (bool)value.m_value;
            return ((IConvertible)value).ToBoolean(null);
        }

        public static implicit operator Integer(Any value)
        {
            if (value.m_value is Integer)
                return (Integer)value.m_value;
            if (value.m_value is int)
                return (int)value.m_value;
            return ((IConvertible)value).ToInt32(null);
        }

        public static implicit operator Float(Any value)
        {
            if (value.m_value is Float)
                return (Float)value.m_value;
            if (value.m_value is float)
                return (float)value.m_value;
            return ((IConvertible)value).ToSingle(null);
        }

        public static implicit operator Double(Any value)
        {
            if (value.m_value is Double)
                return (Double)value.m_value;
            if (value.m_value is double)
                return (double)value.m_value;
            return ((IConvertible)value).ToDouble(null);
        }

        public static Boolean operator ==(Any left, Any right)
        {
            if (left.m_value is null)
                return right.m_value is null;
            if (right.m_value is null)
                return left.m_value is null;
            return left.m_value.Equals(right.m_value);
        }

        public static Boolean operator !=(Any left, Any right)
        {
            return !(left == right);
        }

        public static Any operator +(Any left, Any right)
        {
            if (left.m_value is IConvertible x && right.m_value is IConvertible y)
            {
                object res = 0;
                switch (ResultOf(x.GetTypeCode(), y.GetTypeCode()))
                {
                    case TypeCode.Int32:
                        res = (Integer)(x.ToInt32(null) + y.ToInt32(null));
                        break;
                    case TypeCode.Int64:
                        res = (Long)(x.ToInt64(null) + y.ToInt64(null));
                        break;
                    case TypeCode.Single:
                        res = (Float)x.ToSingle(null) + y.ToSingle(null);
                        break;
                    case TypeCode.Double:
                        res = (Double)x.ToDouble(null) + y.ToDouble(null);
                        break;
                    case TypeCode.String:
                        res = new String(string.Concat(x.ToString(null), y.ToString(null)));
                        break;
                }
                return new Any(res);
            }
            return default(Any);
        }

        public static Any operator -(Any left, Any right)
        {
            if (left.m_value is IConvertible x && right.m_value is IConvertible y)
            {
                object res = 0;
                switch (ResultOf(x.GetTypeCode(), y.GetTypeCode()))
                {
                    case TypeCode.Int32:
                        res = (Integer)(x.ToInt32(null) - y.ToInt32(null));
                        break;
                    case TypeCode.Int64:
                        res = (Long)(x.ToInt64(null) - y.ToInt64(null));
                        break;
                    case TypeCode.Single:
                        res = (Float)x.ToSingle(null) - y.ToSingle(null);
                        break;
                    case TypeCode.Double:
                        res = (Double)x.ToDouble(null) - y.ToDouble(null);
                        break;
                }
                return new Any(res);
            }
            return default(Any);
        }

        public static Any operator *(Any left, Any right)
        {
            if (left.m_value is IConvertible x && right.m_value is IConvertible y)
            {
                object res = 0;
                switch (ResultOf(x.GetTypeCode(), y.GetTypeCode()))
                {
                    case TypeCode.Int32:
                        res = (Integer)(x.ToInt32(null) * y.ToInt32(null));
                        break;
                    case TypeCode.Int64:
                        res = (Long)(x.ToInt64(null) * y.ToInt64(null));
                        break;
                    case TypeCode.Single:
                        res = (Float)x.ToSingle(null) * y.ToSingle(null);
                        break;
                    case TypeCode.Double:
                        res = (Double)x.ToDouble(null) * y.ToDouble(null);
                        break;
                }
                return new Any(res);
            }
            return default(Any);
        }

        public static Any operator /(Any left, Any right)
        {
            if (left.m_value is IConvertible x && right.m_value is IConvertible y)
            {
                object res = 0;
                switch (ResultOf(x.GetTypeCode(), y.GetTypeCode()))
                {
                    case TypeCode.Int32:
                        res = (Integer)(x.ToInt32(null) / y.ToInt32(null));
                        break;
                    case TypeCode.Int64:
                        res = (Long)(x.ToInt64(null) / y.ToInt64(null));
                        break;
                    case TypeCode.Single:
                        res = (Float)x.ToSingle(null) / y.ToSingle(null);
                        break;
                    case TypeCode.Double:
                        res = (Double)x.ToDouble(null) / y.ToDouble(null);
                        break;
                }
                return new Any(res);
            }
            return default(Any);
        }

        public static Any operator %(Any left, Any right)
        {
            if (left.m_value is IConvertible x && right.m_value is IConvertible y)
            {
                object res = 0;
                switch (ResultOf(x.GetTypeCode(), y.GetTypeCode()))
                {
                    case TypeCode.Int32:
                        res = (Integer)(x.ToInt32(null) % y.ToInt32(null));
                        break;
                    case TypeCode.Int64:
                        res = (Long)(x.ToInt64(null) % y.ToInt64(null));
                        break;
                    case TypeCode.Single:
                        res = (Float)x.ToSingle(null) % y.ToSingle(null);
                        break;
                    case TypeCode.Double:
                        res = (Double)x.ToDouble(null) % y.ToDouble(null);
                        break;
                }
                return new Any(res);
            }
            return default(Any);
        }

        public static Boolean operator >(Any left, Any right)
        {
            if (left.m_value is IConvertible x && right.m_value is IConvertible y)
            {
                bool res = false;
                switch (ResultOf(x.GetTypeCode(), y.GetTypeCode()))
                {
                    case TypeCode.Int32:
                        res = (x.ToInt32(null) > y.ToInt32(null));
                        break;
                    case TypeCode.Int64:
                        res = (x.ToInt64(null) > y.ToInt64(null));
                        break;
                    case TypeCode.Single:
                        res = x.ToSingle(null) > y.ToSingle(null);
                        break;
                    case TypeCode.Double:
                        res = x.ToDouble(null) > y.ToDouble(null);
                        break;
                }
                return new Any((Boolean)res);
            }
            return Boolean.False;
        }

        public static Integer operator >>(Any left, int right)
        {
            if (left.m_value is IConvertible x)
            {
                return (x.ToInt32(null) >> right);
            }
            return default(Integer);
        }

        public static Boolean operator <(Any left, Any right)
        {
            if (left.m_value is IConvertible x && right.m_value is IConvertible y)
            {
                bool res = false;
                switch (ResultOf(x.GetTypeCode(), y.GetTypeCode()))
                {
                    case TypeCode.Int32:
                        res = (x.ToInt32(null) < y.ToInt32(null));
                        break;
                    case TypeCode.Int64:
                        res = (x.ToInt64(null) < y.ToInt64(null));
                        break;
                    case TypeCode.Single:
                        res = x.ToSingle(null) < y.ToSingle(null);
                        break;
                    case TypeCode.Double:
                        res = x.ToDouble(null) < y.ToDouble(null);
                        break;
                }
                return new Any((Boolean)res);
            }
            return Boolean.False;
        }

        public static Integer operator <<(Any left, int right)
        {
            if (left.m_value is IConvertible x)
            {
                return (x.ToInt32(null) << right);
            }
            return default(Integer);
        }

        public static Boolean operator <=(Any left, Any right)
        {
            if (left.m_value is IConvertible x && right.m_value is IConvertible y)
            {
                bool res = false;
                switch (ResultOf(x.GetTypeCode(), y.GetTypeCode()))
                {
                    case TypeCode.Int32:
                        res = x.ToInt32(null) <= y.ToInt32(null);
                        break;
                    case TypeCode.Int64:
                        res = x.ToInt64(null) <= y.ToInt64(null);
                        break;
                    case TypeCode.Single:
                        res = x.ToSingle(null) <= y.ToSingle(null);
                        break;
                    case TypeCode.Double:
                        res = x.ToDouble(null) <= y.ToDouble(null);
                        break;
                }
                return new Any((Boolean)res);
            }
            return Boolean.False;
        }

        public static Boolean operator >=(Any left, Any right)
        {
            if (left.m_value is IConvertible x && right.m_value is IConvertible y)
            {
                bool res = false;
                switch (ResultOf(x.GetTypeCode(), y.GetTypeCode()))
                {
                    case TypeCode.Int32:
                        res = x.ToInt32(null) >= y.ToInt32(null);
                        break;
                    case TypeCode.Int64:
                        res = x.ToInt64(null) >= y.ToInt64(null);
                        break;
                    case TypeCode.Single:
                        res = x.ToSingle(null) >= y.ToSingle(null);
                        break;
                    case TypeCode.Double:
                        res = x.ToDouble(null) >= y.ToDouble(null);
                        break;
                }
                return new Any((Boolean)res);
            }
            return Boolean.False;
        }

        public static Any operator ++(Any value)
        {
            if (value.m_value is IConvertible x)
            {
                object res = false;
                switch (x.GetTypeCode())
                {
                    case TypeCode.Int32:
                        res = (Integer)(x.ToInt32(null) + 1);
                        break;
                    case TypeCode.Int64:
                        res = (Long)(x.ToInt64(null) + 1);
                        break;
                    case TypeCode.Single:
                        res = (Float)(x.ToSingle(null) + 1);
                        break;
                    case TypeCode.Double:
                        res = (Double)(x.ToDouble(null) + 1);
                        break;
                }
                return new Any(res);
            }
            return default(Any);
        }

        public static Any operator --(Any value)
        {
            if (value.m_value is IConvertible x)
            {
                object res = false;
                switch (x.GetTypeCode())
                {
                    case TypeCode.Int32:
                        res = (Integer)(x.ToInt32(null) + 1);
                        break;
                    case TypeCode.Int64:
                        res = (Long)(x.ToInt64(null) + 1);
                        break;
                    case TypeCode.Single:
                        res = (Float)(x.ToSingle(null) + 1);
                        break;
                    case TypeCode.Double:
                        res = (Double)(x.ToDouble(null) + 1);
                        break;
                }
                return new Any(res);
            }
            return default(Any);
        }

        public static Any operator +(Any value)
        {
            if (value.m_value is IConvertible x)
            {
                object res = false;
                switch (x.GetTypeCode())
                {
                    case TypeCode.Int32:
                        res = (Integer)(+x.ToInt32(null));
                        break;
                    case TypeCode.Int64:
                        res = (Long)(+x.ToInt64(null));
                        break;
                    case TypeCode.Single:
                        res = (Float)(+x.ToSingle(null));
                        break;
                    case TypeCode.Double:
                        res = (Double)(+x.ToDouble(null));
                        break;
                }
                return new Any(res);
            }
            return default(Any);
        }

        public static Any operator -(Any value)
        {
            if (value.m_value is IConvertible x)
            {
                object res = false;
                switch (x.GetTypeCode())
                {
                    case TypeCode.Int32:
                        res = (Integer)(-x.ToInt32(null));
                        break;
                    case TypeCode.Int64:
                        res = (Long)(-x.ToInt64(null));
                        break;
                    case TypeCode.Single:
                        res = (Float)(-x.ToSingle(null));
                        break;
                    case TypeCode.Double:
                        res = (Double)(-x.ToDouble(null));
                        break;
                }
                return new Any(res);
            }
            return default(Any);
        }

        public static Integer operator &(Any left, Any right)
        {
            if (left.m_value is IConvertible x && right.m_value is IConvertible y)
            {
                return x.ToInt32(null) & y.ToInt32(null);
            }
            return default(Integer);
        }

        public static Integer operator |(Any left, Any right)
        {
            if (left.m_value is IConvertible x && right.m_value is IConvertible y)
            {
                return x.ToInt32(null) | y.ToInt32(null);
            }
            return default(Integer);
        }

        public static Integer operator ^(Any left, Any right)
        {
            if (left.m_value is IConvertible x && right.m_value is IConvertible y)
            {
                return x.ToInt32(null) ^ y.ToInt32(null);
            }
            return default(Integer);
        }

        public static Integer operator ~(Any value)
        {
            if (value.m_value is IConvertible x)
            {
                return ~x.ToInt32(null);
            }
            return default(Integer);
        }

        public static TypeCode ResultOf(TypeCode left, TypeCode right)
        {
            switch (left)
            {
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                    switch (right)
                    {
                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                            return TypeCode.Int32;
                        default:
                            return right;
                    }
                case TypeCode.Int32:
                    switch (right)
                    {
                        case TypeCode.Byte:
                        case TypeCode.SByte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                            return TypeCode.Int32;
                        case TypeCode.UInt32:
                            return TypeCode.Int64;
                        default:
                            return right;
                    }
                case TypeCode.UInt32:
                    switch (right)
                    {
                        case TypeCode.Byte:
                        case TypeCode.UInt16:
                        case TypeCode.UInt32:
                            return TypeCode.UInt32;
                        case TypeCode.SByte:
                        case TypeCode.Int32:
                        case TypeCode.Int16:
                            return TypeCode.Int64;
                        default:
                            return right;
                    }
                case TypeCode.Int64:
                    switch (right)
                    {
                        case TypeCode.Byte:
                        case TypeCode.SByte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                            return TypeCode.Int64;
                        default:
                            return right;
                    }
                case TypeCode.UInt64:
                    switch (right)
                    {
                        case TypeCode.Byte:
                        case TypeCode.SByte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                            return TypeCode.Int64;
                        default:
                            return right;
                    }
                case TypeCode.Single:
                    if (right == TypeCode.Double)
                        return right;
                    return left;
                default:
                    return left;

            }
        }

        [SpecialName]
        public static Any op_Implicit(object value)
        {
            if (value is ValueType)
            {
                var type = value.GetType();
                if (type.IsPrimitive)
                {
                    switch (value)
                    {
                        case bool b:
                            value = (Boolean)b;
                            break;
                        case sbyte b:
                            value = (Byte)b;
                            break;
                        case short s:
                            value = (Short)s;
                            break;
                        case int i:
                            value = ((Integer)i);
                            break;
                        case long l:
                            value = (Long)l;
                            break;
                        case float f:
                            value = (Float)f;
                            break;
                        case double d:
                            value = (Double)d;
                            break;
                    }
                    return new Any(value);
                }
                // value is already any type
                if (value is Any)
                    return (Any)value;
                return new Any(value, type);
            }
            return new Any(value);
        }

        [SpecialName]
        public static Any op_Implicit(string value)
        {
            return new Any((String)value);
        }

        [SpecialName]
        public static object op_Explicit(Any value)
        {
            return value.m_value;
        }
    }
}