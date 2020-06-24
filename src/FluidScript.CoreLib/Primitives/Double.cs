using System;

namespace FluidScript
{
    /// <summary>
    /// Represents a double-precision floating-point number.
    /// </summary>
    [Serializable]
    [Runtime.Register(nameof(Double))]
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    [System.Runtime.InteropServices.ComVisible(true)]
    public
#if LATEST_VS
        readonly
#endif
        struct Double : IFSObject, IConvertible, IFormattable, Runtime.IValueBox<double>
    {
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        internal readonly double m_value;

        [Runtime.Register(nameof(NaN))]
        public static readonly Double NaN = new Double(double.NaN);

        /// <summary>
        /// Initializes a new instance of the <see cref="Double"/>
        /// </summary>
        public Double(double value)
        {
            m_value = value;
        }

        /// <inheritdoc/>
        [Runtime.Register("toString")]
        String IFSObject.ToString()
        {
            return m_value.ToString();
        }

        /// <inheritdoc/>
        [Runtime.Register("hashCode")]
        Integer IFSObject.GetHashCode()
        {
            return m_value.GetHashCode();
        }

        /// <inheritdoc/>
        [Runtime.Register("equals")]
        Boolean IFSObject.Equals(object obj)
        {
            return obj is Double d &&
                  m_value == d.m_value ? Boolean.True : Boolean.False;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj is Double d &&
                  m_value == d.m_value;
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="obj"> The object to compare with the current instance.</param>
        /// <returns>true if obj and this instance are the same type and represent the same value;
        ///  otherwise, false.</returns>
        public Boolean Equals(Double obj)
        {
            return m_value == obj.m_value? Boolean.True : Boolean.False;
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
        public static Double Parse(object value)
        {
            if (!(value is IConvertible c))
                throw new InvalidCastException(nameof(value));
            return new Double(c.ToDouble(null));
        }

        #region Convertible
        ///<inheritdoc/>
        public TypeCode GetTypeCode()
        {
            return TypeCode.Double;
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
            return m_value;
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

        public static implicit operator Double(double value) => new Double(value);

        public static implicit operator Double(Byte value) => new Double(value.m_value);

        public static implicit operator Double(Short value) => new Double(value.m_value);

        public static implicit operator Double(Char value) => new Double(value.m_value);

        public static implicit operator Double(Integer value) => new Double(value.m_value);

        public static implicit operator Double(Long value) => new Double(value.m_value);

        public static implicit operator Double(Float value) => new Double(value.m_value);

        public static implicit operator double(Double integer) => integer.m_value;

        public static Double operator +(Double left, Double right)
        {
            return new Double(left.m_value + right.m_value);
        }

        public static Double operator +(Double value)
        {
            return new Double(+value.m_value);
        }

        public static Double operator -(Double left, Double right)
        {
            return new Double(left.m_value - right.m_value);
        }

        public static Double operator -(Double value)
        {
            return new Double(-value.m_value);
        }

        public static Double operator *(Double left, Double right)
        {
            return new Double(left.m_value * right.m_value);
        }

        public static Double operator /(Double left, Double right)
        {
            return new Double(left.m_value / right.m_value);
        }

        public static Double operator %(Double left, Double right)
        {
            return new Double(left.m_value % right.m_value);
        }

        public static Boolean operator >(Double left, Double right)
        {
            return left.m_value > right.m_value ? Boolean.True : Boolean.False;
        }

        public static Boolean operator >=(Double left, Double right)
        {
            return left.m_value >= right.m_value ? Boolean.True : Boolean.False;
        }

        public static Boolean operator <(Double left, Double right)
        {
            return left.m_value < right.m_value ? Boolean.True : Boolean.False;
        }

        public static Boolean operator <=(Double left, Double right)
        {
            return left.m_value <= right.m_value ? Boolean.True : Boolean.False;
        }

        public static Boolean operator ==(Double left, Double right)
        {
            return left.m_value == right.m_value ? Boolean.True : Boolean.False;
        }

        public static Boolean operator !=(Double left, Double right)
        {
            return left.m_value != right.m_value ? Boolean.True : Boolean.False;
        }

        public static Double operator ++(Double value)
        {
            return new Double(value.m_value + 1);
        }

        public static Double operator --(Double value)
        {
            return new Double(value.m_value - 1);
        }
    }
}
