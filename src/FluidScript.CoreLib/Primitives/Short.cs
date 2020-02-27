namespace FluidScript
{
    /// <summary>
    /// Represents a 16-bit signed integer.
    /// </summary>
    [System.Serializable]
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    [System.Runtime.InteropServices.ComVisible(true)]
    public
#if LATEST_VS
        readonly
#endif
        struct Short : IFSObject, System.IConvertible
    {
        [System.Diagnostics.DebuggerBrowsable(0)]
        internal readonly short m_value;

        /// <summary>
        /// Initializes a new instance of the <see cref="Short"/>
        /// </summary>
        public Short(short value)
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
        Integer IFSObject.__HashCode()
        {
            return m_value.GetHashCode();
        }

        /// <inheritdoc/>
        [Runtime.Register("equals")]
        Boolean IFSObject.Equals(IFSObject obj)
        {
            return obj is Short s &&
                  m_value == s.m_value ? Boolean.True : Boolean.False;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj is Short s &&
                  m_value == s.m_value;
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="obj"> The object to compare with the current instance.</param>
        /// <returns>true if obj and this instance are the same type and represent the same value;
        ///  otherwise, false.</returns>
        public Boolean Equals(Short obj)
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

        #region Convertible
        ///<inheritdoc/>
        public System.TypeCode GetTypeCode()
        {
            return System.TypeCode.Int16;
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
            return m_value;
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

        public static implicit operator Short(short value) => new Short(value);

        public static implicit operator Short(Byte value) => new Short(value.m_value);

        public static implicit operator short(Short value) => value.m_value;
    }
}
