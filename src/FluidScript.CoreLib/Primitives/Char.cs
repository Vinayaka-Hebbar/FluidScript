using System;

namespace FluidScript
{
    /// <summary>
    /// Represents a character as a UTF-16 code unit.
    /// </summary>
    [Runtime.Register(nameof(Char))]
    [Serializable]
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    [System.Runtime.InteropServices.ComVisible(true)]
    public
#if LATEST_VS
        readonly
#endif
        struct Char : IFSObject, IConvertible, Runtime.IValueBox<char>
    {
        /// <summary>
        /// Min char Value
        /// </summary>
        [Runtime.Register(nameof(MinValue))]
        public static readonly Char MinValue = new Char(char.MinValue);

        /// <summary>
        /// Max char Value
        /// </summary>
        [Runtime.Register(nameof(MinValue))]
        public static readonly Char MaxValue = new Char(char.MaxValue);

        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        internal readonly char m_value;

        /// <summary>
        /// Initializes a new instance of the <see cref="Char"/>
        /// </summary>
        public Char(char value)
        {
            m_value = value;
        }

        /// <summary>
        /// returns the hashCode() for the instance
        /// </summary>
        [Runtime.Register("hashCode")]
        Integer IFSObject.GetHashCode()
        {
            return m_value.GetHashCode();
        }

        /// <summary>
        /// converts to string
        /// </summary>
        [Runtime.Register("toString")]
        String IFSObject.ToString()
        {
            return m_value.ToString();
        }

        /// <summary>
        /// checks <paramref name="obj"/> and <see cref="Integer"/> are equals
        /// </summary>
        [Runtime.Register("equals")]
        Boolean IFSObject.Equals(object obj)
        {
            return obj is Char c &&
                  m_value == c.m_value;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj is Char c &&
                  m_value == c.m_value ? Boolean.True : Boolean.False;
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="obj"> The object to compare with the current instance.</param>
        /// <returns>true if obj and this instance are the same type and represent the same value;
        ///  otherwise, false.</returns>
        public Boolean Equals(Char obj)
        {
            return m_value == obj.m_value ? Boolean.True : Boolean.False;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return m_value | (m_value << 16);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return m_value.ToString();
        }

        [Runtime.Register("parse")]
        public static Char Parse(object value)
        {
            if (!(value is IConvertible c))
                throw new InvalidCastException(nameof(value));
            return new Char(c.ToChar(null));
        }

        #region Convertible
        ///<inheritdoc/>
        public TypeCode GetTypeCode()
        {
            return TypeCode.Char;
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
            return m_value;
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

        /// <summary>
        /// Implicit Convert from <see cref="char"/> to <see cref="Char"/>
        /// </summary>
        public static implicit operator Char(char value) => new Char(value);

        /// <summary>
        /// Implicit Convert from <see cref="Char"/> to <see cref="Integer"/>
        /// </summary>
        public static implicit operator Integer(Char value) => new Integer(value);

        /// <summary>
        /// Implicit Convert from <see cref="Char"/> to <see cref="char"/>
        /// </summary>
        public static implicit operator char(Char value) => value.m_value;

        /// <summary>
        /// Implicit Convert from <see cref="Char"/> to <see cref="int"/>
        /// </summary>
        public static implicit operator int(Char value) => value.m_value;

        public static Integer operator +(Char left, Integer right)
        {
            return new Integer(left.m_value + right.m_value);
        }

        public static Integer operator -(Char left, Integer right)
        {
            return new Integer(left.m_value - right.m_value);
        }
    }
}
