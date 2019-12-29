namespace FluidScript
{
    /// <summary>
    /// Represents a character as a UTF-16 code unit.
    /// </summary>
    [System.Runtime.InteropServices.ComVisible(true)]
    public
#if LATEST_VS
        readonly
#endif
        struct Char : IFSObject, System.IConvertible
    {
        /// <summary>
        /// Min char Value
        /// </summary>
        public static readonly Char MinValue = new Char(char.MinValue);

        /// <summary>
        /// Max char Value
        /// </summary>
        public static readonly Char MaxValue = new Char(char.MaxValue);

        [System.Diagnostics.DebuggerBrowsable(0)]
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
        Integer IFSObject.HashCode()
        {
            return m_value.GetHashCode();
        }

        /// <summary>
        /// converts to string
        /// </summary>
        [Runtime.Register("toString")]
        String IFSObject.__ToString()
        {
            return m_value.ToString();
        }

        /// <summary>
        /// checks <paramref name="other"/> and <see cref="Integer"/> are equals
        /// </summary>
        [Runtime.Register("equals")]
        Boolean IFSObject.__Equals(IFSObject other)
        {
            return other is Char c &&
                  m_value == c.m_value;
        }

        /// <inheritdoc/>
        public override bool Equals(object other)
        {
            return other is Char c &&
                  m_value == c.m_value ? Boolean.True : Boolean.False;
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

        #region Convertible
        System.TypeCode System.IConvertible.GetTypeCode()
        {
            return System.TypeCode.Char;
        }

        string System.IConvertible.ToString(System.IFormatProvider provider)
        {
            return System.Convert.ToString(provider);
        }

        /// <internalonly/>
        bool System.IConvertible.ToBoolean(System.IFormatProvider provider)
        {
            return System.Convert.ToBoolean(m_value);
        }

        /// <internalonly/>
        char System.IConvertible.ToChar(System.IFormatProvider provider)
        {
            return m_value;
        }

        /// <internalonly/>
        sbyte System.IConvertible.ToSByte(System.IFormatProvider provider)
        {
            return System.Convert.ToSByte(m_value);
        }

        /// <internalonly/>
        byte System.IConvertible.ToByte(System.IFormatProvider provider)
        {
            return System.Convert.ToByte(m_value);
        }

        /// <internalonly/>
        short System.IConvertible.ToInt16(System.IFormatProvider provider)
        {
            return System.Convert.ToInt16(m_value);
        }

        /// <internalonly/>
        ushort System.IConvertible.ToUInt16(System.IFormatProvider provider)
        {
            return System.Convert.ToUInt16(m_value);
        }

        /// <internalonly/>
        int System.IConvertible.ToInt32(System.IFormatProvider provider)
        {
            return System.Convert.ToInt32(m_value);
        }

        /// <internalonly/>
        uint System.IConvertible.ToUInt32(System.IFormatProvider provider)
        {
            return System.Convert.ToUInt32(m_value);
        }

        /// <internalonly/>
        long System.IConvertible.ToInt64(System.IFormatProvider provider)
        {
            return System.Convert.ToInt64(m_value);
        }

        /// <internalonly/>
        ulong System.IConvertible.ToUInt64(System.IFormatProvider provider)
        {
            return System.Convert.ToUInt64(m_value);
        }

        /// <internalonly/>
        float System.IConvertible.ToSingle(System.IFormatProvider provider)
        {
            return System.Convert.ToSingle(m_value);
        }

        /// <internalonly/>
        double System.IConvertible.ToDouble(System.IFormatProvider provider)
        {
            return System.Convert.ToDouble(m_value);
        }

        /// <internalonly/>
        decimal System.IConvertible.ToDecimal(System.IFormatProvider provider)
        {
            return System.Convert.ToDecimal(m_value);
        }

        /// <internalonly/>
        System.DateTime System.IConvertible.ToDateTime(System.IFormatProvider provider)
        {
            throw new System.InvalidCastException("Invalid Cast From Integer To DateTime");
        }

        /// <internalonly/>
        object System.IConvertible.ToType(System.Type type, System.IFormatProvider provider)
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
