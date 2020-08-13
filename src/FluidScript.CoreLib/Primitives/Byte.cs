using System;

namespace FluidScript
{
    /// <summary>
    ///  Represents an 8-bit signed integer.
    /// </summary>
    [Runtime.Register(nameof(Byte))]
    [Serializable]
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    [System.Runtime.InteropServices.ComVisible(true)]
    public
#if LATEST_VS
        readonly
#endif
        struct Byte : IFSObject, IConvertible, IFormattable,  Runtime.IValueBox<sbyte>
    {
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        internal readonly sbyte m_value;

        /// <summary>
        /// Initializes a new instance of the <see cref="Byte"/>
        /// </summary>
        public Byte(sbyte value)
        {
            m_value = value;
        }

        /// <inheritdoc/>
        [Runtime.Register("toString")]
        public String StringValue()
        {
            return m_value.ToString();
        }

        /// <inheritdoc/>
        [Runtime.Register("hashCode")]
        public Integer HashCode()
        {
            return m_value.GetHashCode();
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj is Byte b &&
                  m_value == b.m_value;
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="obj"> The object to compare with the current instance.</param>
        /// <returns>true if obj and this instance are the same type and represent the same value;
        ///  otherwise, false.</returns>
        [Runtime.Register("equals")]
        public Boolean Equals(Byte obj)
        {
            return m_value == obj.m_value ? Boolean.True : Boolean.False;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return m_value;
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
        public static Byte Parse(object value)
        {
            if (!(value is IConvertible c))
                throw new InvalidCastException(nameof(value));
            return new Byte(c.ToSByte(null));
        }

        #region Convertible
        ///<inheritdoc/>
        public TypeCode GetTypeCode()
        {
            return TypeCode.SByte;
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
            return m_value;
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

        public static implicit operator Byte(sbyte value) => new Byte(value);
        
        public static implicit operator sbyte(Byte value) => value.m_value;

        public static Boolean operator ==(Byte left, Byte right)
        {
            return left.Equals(right);
        }

        public static Boolean operator !=(Byte left, Byte right)
        {
            return !left.Equals(right);
        }
    }
}
