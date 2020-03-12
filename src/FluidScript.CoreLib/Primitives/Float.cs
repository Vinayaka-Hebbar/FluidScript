namespace FluidScript
{
    /// <summary>
    /// Represents a single-precision floating-point number.
    /// </summary>
    [System.Serializable]
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    [System.Runtime.InteropServices.ComVisible(true)]
    public
#if LATEST_VS
        readonly
#endif
        struct Float : IFSObject, System.IConvertible
    {
        [System.Diagnostics.DebuggerBrowsable(0)]
        internal readonly float m_value;

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

        #region Convertible
        ///<inheritdoc/>
        public System.TypeCode GetTypeCode()
        {
            return System.TypeCode.Single;
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
            return m_value;
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

        public static implicit operator Float(float value) => new Float(value);

        public static implicit operator Float(Byte value) => new Float(value.m_value);

        public static implicit operator Float(Short value) => new Float(value.m_value);

        public static implicit operator Float(Char value) => new Float(value.m_value);

        public static implicit operator Float(Integer value) => new Float(value.m_value);

        public static implicit operator Float(Long value) => new Float(value.m_value);

        public static implicit operator float(Float value) => value.m_value;

        public static implicit operator double(Float value) => value.m_value;

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
