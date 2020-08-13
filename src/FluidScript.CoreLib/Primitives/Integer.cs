using System;

namespace FluidScript
{
    /// <summary>
    /// Represents a 32-bit signed integer.
    /// </summary>
    [Runtime.Register(nameof(Integer))]
    [Serializable]
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    [System.Runtime.InteropServices.ComVisible(true)]
    public
#if LATEST_VS
        readonly
#endif
        struct Integer : IFSObject, IConvertible, IFormattable, Runtime.IValueBox<int>
    {
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        internal readonly int m_value;

        /// <summary>
        /// Initializes a new instance of the <see cref="Integer"/>
        /// </summary>
        public Integer(int value)
        {
            m_value = value;
        }

        /// <inheritdoc/>
        [Runtime.Register("hashCode")]
        public Integer HashCode()
        {
            return m_value.GetHashCode();
        }

        /// <inheritdoc/>
        [Runtime.Register("toString")]
        public String StringValue()
        {
            return m_value.ToString();
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj is Integer integer &&
                  m_value == integer.m_value;
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="obj"> The object to compare with the current instance.</param>
        /// <returns>true if obj and this instance are the same type and represent the same value;
        ///  otherwise, false.</returns>
        [Runtime.Register("equals")]
        public Boolean Equals(Integer obj)
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

        [Runtime.Register("parse")]
        public static Integer Parse(object value)
        {
            if (!(value is IConvertible c))
                throw new InvalidCastException(nameof(value));
            return new Integer(c.ToInt32(null));
        }

        #region Convertible
        ///<inheritdoc/>
        public TypeCode GetTypeCode()
        {
            return TypeCode.Int32;
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
            return m_value;
        }

        /// <internalonly/>
        uint IConvertible.ToUInt32(IFormatProvider provider)
        {
            return Convert.ToUInt32(m_value);
        }

        /// <internalonly/>
        long IConvertible.ToInt64(IFormatProvider provider)
        {
            return Convert.ToInt64(m_value);
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

        public static implicit operator Integer(int value) => new Integer(value);

        public static implicit operator Integer(Byte value) => new Integer(value.m_value);

        public static implicit operator Integer(Short value) => new Integer(value.m_value);

        public static implicit operator Integer(Char value) => new Integer(value.m_value);

        #region System Implicit
        public static implicit operator int(Integer value) => value.m_value;

        public static implicit operator long(Integer value) => value.m_value;

        public static implicit operator float(Integer value) => value.m_value;

        public static implicit operator double(Integer value) => value.m_value;
        #endregion

        public static Integer operator +(Integer left, Integer right)
        {
            return new Integer(left.m_value + right.m_value);
        }

        public static Integer operator +(Integer value)
        {
            return new Integer(+value.m_value);
        }

        public static Integer operator -(Integer left, Integer right)
        {
            return new Integer(left.m_value - right.m_value);
        }

        public static Integer operator -(Integer value)
        {
            return new Integer(-value.m_value);
        }

        public static Integer operator *(Integer left, Integer right)
        {
            return new Integer(left.m_value * right.m_value);
        }

        public static Integer operator /(Integer left, Integer right)
        {
            return new Integer(left.m_value / right.m_value);
        }

        public static Integer operator %(Integer left, Integer right)
        {
            return new Integer(left.m_value % right.m_value);
        }

        public static Boolean operator >(Integer left, Integer right)
        {
            return left.m_value > right.m_value ? Boolean.True : Boolean.False;
        }

        public static Boolean operator >=(Integer left, Integer right)
        {
            return left.m_value >= right.m_value ? Boolean.True : Boolean.False;
        }

        public static Integer operator >>(Integer left, int right)
        {
            return new Integer(left.m_value >> right);
        }

        public static Boolean operator <(Integer left, Integer right)
        {
            return left.m_value < right.m_value ? Boolean.True : Boolean.False;
        }

        public static Boolean operator <=(Integer left, Integer right)
        {
            return left.m_value <= right.m_value ? Boolean.True : Boolean.False;
        }

        public static Integer operator <<(Integer left, int right)
        {
            return new Integer(left.m_value << right);
        }

        public static Boolean operator ==(Integer left, Integer right)
        {
            return left.m_value == right.m_value ? Boolean.True : Boolean.False;
        }

        public static Boolean operator !=(Integer left, Integer right)
        {
            return left.m_value != right.m_value ? Boolean.True : Boolean.False;
        }

        public static Integer operator ++(Integer value)
        {
            return new Integer(value.m_value + 1);
        }

        public static Integer operator --(Integer value)
        {
            return new Integer(value.m_value - 1);
        }

        public static Integer operator &(Integer left, Integer right)
        {
            return new Integer(left.m_value & right.m_value);
        }

        public static Integer operator |(Integer left, Integer right)
        {
            return new Integer(left.m_value | right.m_value);
        }

        /// <summary>
        /// op_ExclusiveOr implementation
        /// </summary>
        public static Integer operator ^(Integer left, Integer right)
        {
            return new Integer(left.m_value ^ right.m_value);
        }

        public static Integer operator ~(Integer value)
        {
            return new Integer(~value.m_value);
        }
    }
}
