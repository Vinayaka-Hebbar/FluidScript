using System;

namespace FluidScript
{
    /// <summary>
    /// Represents a single-precision floating-point number.
    /// </summary>
    [Runtime.Register(nameof(Float))]
    [Serializable]
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    [System.Runtime.InteropServices.ComVisible(true)]
    public
#if LATEST_VS
        readonly
#endif
        struct Float : IFSObject, IConvertible, IFormattable, Runtime.IValueBox<float>
    {
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        internal readonly float m_value;

        [Runtime.Register(nameof(NaN))]
        public static readonly Float NaN = new Float(float.NaN);

        /// <summary>
        /// Initializes a new instance of the <see cref="Float"/>
        /// </summary>
        public Float(float value)
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
            return obj is Float f &&
                  m_value == f.m_value;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj is Float f &&
                  m_value == f.m_value;
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="obj"> The object to compare with the current instance.</param>
        /// <returns>true if obj and this instance are the same type and represent the same value;
        ///  otherwise, false.</returns>
        public Boolean Equals(Float obj)
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
        public static Float Parse(object value)
        {
            if (!(value is IConvertible c))
                throw new InvalidCastException(nameof(value));
            return new Float(c.ToSingle(null));
        }

        #region Convertible
        ///<inheritdoc/>
        public TypeCode GetTypeCode()
        {
            return TypeCode.Single;
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
            return m_value;
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

        public static implicit operator Float(float value) => new Float(value);

        public static implicit operator Float(Byte value) => new Float(value.m_value);

        public static implicit operator Float(Short value) => new Float(value.m_value);

        public static implicit operator Float(Char value) => new Float(value.m_value);

        public static implicit operator Float(Integer value) => new Float(value.m_value);

        public static implicit operator Float(Long value) => new Float(value.m_value);

        public static implicit operator float(Float value) => value.m_value;

        public static Float operator +(Float left, Float right)
        {
            return new Float(left.m_value + right.m_value);
        }

        public static Float operator +(Float value)
        {
            return new Float(+value.m_value);
        }

        public static Float operator -(Float left, Float right)
        {
            return new Float(left.m_value - right.m_value);
        }

        public static Float operator -(Float value)
        {
            return new Float(-value.m_value);
        }

        public static Float operator *(Float left, Float right)
        {
            return new Float(left.m_value * right.m_value);
        }

        public static Float operator /(Float left, Float right)
        {
            return new Float(left.m_value / right.m_value);
        }

        public static Float operator %(Float left, Float right)
        {
            return new Float(left.m_value % right.m_value);
        }

        public static Boolean operator >(Float left, Float right)
        {
            return left.m_value > right.m_value ? Boolean.True : Boolean.False;
        }

        public static Boolean operator >=(Float left, Float right)
        {
            return left.m_value >= right.m_value ? Boolean.True : Boolean.False;
        }

        public static Boolean operator <(Float left, Float right)
        {
            return left.m_value < right.m_value ? Boolean.True : Boolean.False;
        }

        public static Boolean operator <=(Float left, Float right)
        {
            return left.m_value <= right.m_value ? Boolean.True : Boolean.False;
        }

        public static Boolean operator ==(Float left, Float right)
        {
            return left.m_value == right.m_value ? Boolean.True : Boolean.False;
        }

        public static Boolean operator !=(Float left, Float right)
        {
            return left.m_value != right.m_value ? Boolean.True : Boolean.False;
        }

        public static Float operator ++(Float value)
        {
            return new Float(value.m_value + 1);
        }

        public static Float operator --(Float value)
        {
            return new Float(value.m_value + 1);
        }
    }
}
