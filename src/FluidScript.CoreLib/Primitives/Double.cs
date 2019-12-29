namespace FluidScript
{
    /// <summary>
    /// Represents a double-precision floating-point number.
    /// </summary>
    [System.Serializable]
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    [System.Runtime.InteropServices.ComVisible(true)]
    public
#if LATEST_VS
        readonly
#endif
        struct Double : IFSObject, System.IConvertible
    {
        [System.Diagnostics.DebuggerBrowsable(0)]
        internal readonly double m_value;

        /// <summary>
        /// Initializes a new instance of the <see cref="Double"/>
        /// </summary>
        public Double(double value)
        {
            m_value = value;
        }

        /// <inheritdoc/>
        [Runtime.Register("toString")]
        String IFSObject.__ToString()
        {
            return m_value.ToString();
        }

        /// <inheritdoc/>
        [Runtime.Register("hashCode")]
        Integer IFSObject.HashCode()
        {
            return m_value.GetHashCode();
        }

        /// <inheritdoc/>
        [Runtime.Register("equals")]
        Boolean IFSObject.__Equals(IFSObject other)
        {
            return other is Double d &&
                  m_value == d.m_value ? Boolean.True : Boolean.False;
        }

        /// <inheritdoc/>
        public override bool Equals(object other)
        {
            return other is Double d &&
                  m_value == d.m_value;
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

        #region Convertible
        System.TypeCode System.IConvertible.GetTypeCode()
        {
            return System.TypeCode.Double;
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
            return System.Convert.ToChar(m_value);
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
            return m_value;
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

        public static implicit operator Double(double value) => new Double(value);

        public static implicit operator double(Double integer) => integer.m_value;

        public static implicit operator Double(Byte value) => new Double(value.m_value);

        public static implicit operator Double(Short value) => new Double(value.m_value);

        public static implicit operator Double(Char value) => new Double(value.m_value);

        public static implicit operator Double(Integer value) => new Double(value.m_value);

        public static implicit operator Double(Long value) => new Double(value.m_value);

        public static implicit operator Double(Float value) => new Double(value.m_value);

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
            return new Double(value.m_value + 1);
        }
    }
}
