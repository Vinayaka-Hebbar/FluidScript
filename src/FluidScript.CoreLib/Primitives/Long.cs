using FluidScript.Runtime;
using System;

namespace FluidScript
{
    /// <summary>
    /// Represents a 64-bit signed integer.
    /// </summary>
    [Serializable]
    [Register(nameof(Long))]
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    [System.Runtime.InteropServices.ComVisible(true)]
    public
#if LATEST_VS
        readonly
#endif
        struct Long : IFSObject, IConvertible, IFormattable
    {
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        internal readonly long m_value;

        /// <summary>
        /// Initializes a new instance of the <see cref="Long"/>
        /// </summary>
        public Long(long value)
        {
            m_value = value;
        }

        /// <inheritdoc/>
        [Register("toString")]
        public String StringValue()
        {
            return m_value.ToString();
        }

        /// <inheritdoc/>
        [Register("hashCode")]
        public Integer HashCode()
        {
            return m_value.GetHashCode();
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj is Long l &&
                  m_value == l.m_value;
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="obj"> The object to compare with the current instance.</param>
        /// <returns>true if obj and this instance are the same type and represent the same value;
        ///  otherwise, false.</returns>
        [Register("equals")]
        public Boolean Equals(Long obj)
        {
            return m_value == obj.m_value ? Boolean.True : Boolean.False;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return m_value.GetHashCode();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return m_value.ToString();
        }

        string IFormattable.ToString(string format, IFormatProvider provider)
        {
            return m_value.ToString(format, provider);
        }

        [Register("parse")]
        public static Long Parse(object value)
        {
            if (!(value is IConvertible c))
                throw new InvalidCastException(nameof(value));
            return new Long(c.ToInt64(null));
        }

        #region Convertible
        ///<inheritdoc/>
        public TypeCode GetTypeCode()
        {
            return TypeCode.Int64;
        }

        string IConvertible.ToString(IFormatProvider provider)
        {
            return Convert.ToString(provider);
        }

        /// <internalonly/>
        bool IConvertible.ToBoolean(IFormatProvider provider)
        {
            return Convert.ToBoolean(m_value);
        }

        /// <internalonly/>
        char IConvertible.ToChar(IFormatProvider provider)
        {
            return Convert.ToChar(m_value);
        }

        /// <internalonly/>
        sbyte IConvertible.ToSByte(IFormatProvider provider)
        {
            return Convert.ToSByte(m_value);
        }

        /// <internalonly/>
        byte IConvertible.ToByte(IFormatProvider provider)
        {
            return Convert.ToByte(m_value);
        }

        /// <internalonly/>
        short IConvertible.ToInt16(IFormatProvider provider)
        {
            return Convert.ToInt16(m_value);
        }

        /// <internalonly/>
        ushort IConvertible.ToUInt16(IFormatProvider provider)
        {
            return Convert.ToUInt16(m_value);
        }

        /// <internalonly/>
        int IConvertible.ToInt32(IFormatProvider provider)
        {
            return Convert.ToInt32(m_value);
        }

        /// <internalonly/>
        uint IConvertible.ToUInt32(IFormatProvider provider)
        {
            return Convert.ToUInt32(m_value);
        }

        /// <internalonly/>
        long IConvertible.ToInt64(IFormatProvider provider)
        {
            return m_value;
        }

        /// <internalonly/>
        ulong IConvertible.ToUInt64(IFormatProvider provider)
        {
            return Convert.ToUInt64(m_value);
        }

        /// <internalonly/>
        float IConvertible.ToSingle(IFormatProvider provider)
        {
            return Convert.ToSingle(m_value);
        }

        /// <internalonly/>
        double IConvertible.ToDouble(IFormatProvider provider)
        {
            return Convert.ToDouble(m_value);
        }

        /// <internalonly/>
        decimal IConvertible.ToDecimal(IFormatProvider provider)
        {
            return Convert.ToDecimal(m_value);
        }

        /// <internalonly/>
        DateTime IConvertible.ToDateTime(IFormatProvider provider)
        {
            throw new InvalidCastException("Invalid Cast From Integer To DateTime");
        }

        /// <internalonly/>
        object IConvertible.ToType(Type type, IFormatProvider provider)
        {
            return m_value;
        }
        #endregion

        public static implicit operator Long(long value) => new Long(value);

        public static implicit operator Long(Byte value) => new Long(value.m_value);

        public static implicit operator Long(Short value) => new Long(value.m_value);

        public static implicit operator Long(Char value) => new Long(value.m_value);

        public static implicit operator Long(Integer value) => new Long(value.m_value);

        public static implicit operator long(Long value) => value.m_value;

        public static Long operator +(Long left, Long right)
        {
            return new Long(left.m_value + right.m_value);
        }

        public static Long operator -(Long left, Long right)
        {
            return new Long(left.m_value - right.m_value);
        }

        public static Long operator *(Long left, Long right)
        {
            return new Long(left.m_value * right.m_value);
        }

        public static Long operator /(Long left, Integer right)
        {
            return new Long(left.m_value / right.m_value);
        }

        public static Long operator %(Long left, Long right)
        {
            return new Long(left.m_value % right.m_value);
        }

        public static Boolean operator >(Long left, Long right)
        {
            return left.m_value > right.m_value ? Boolean.True : Boolean.False;
        }

        public static Boolean operator >=(Long left, Long right)
        {
            return left.m_value >= right.m_value ? Boolean.True : Boolean.False;
        }

        public static Long operator >>(Long left, int right)
        {
            return new Long(left.m_value >> right);
        }

        public static Boolean operator <(Long left, Long right)
        {
            return left.m_value < right.m_value ? Boolean.True : Boolean.False;
        }

        public static Boolean operator <=(Long left, Long right)
        {
            return left.m_value <= right.m_value ? Boolean.True : Boolean.False;
        }

        public static Long operator <<(Long left, int right)
        {
            return new Long(left.m_value << right);
        }

        public static Boolean operator ==(Long left, Long right)
        {
            return left.m_value == right.m_value ? Boolean.True : Boolean.False;
        }

        public static Boolean operator !=(Long left, Long right)
        {
            return left.m_value != right.m_value ? Boolean.True : Boolean.False;
        }

        public static Long operator ++(Long value)
        {
            return new Long(value.m_value + 1);
        }

        public static Long operator --(Long value)
        {
            return new Long(value.m_value - 1);
        }
    }
}
