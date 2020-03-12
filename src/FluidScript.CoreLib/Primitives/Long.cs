﻿namespace FluidScript
{
    /// <summary>
    /// Represents a 64-bit signed integer.
    /// </summary>
    [System.Serializable]
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    [System.Runtime.InteropServices.ComVisible(true)]
    public
#if LATEST_VS
        readonly
#endif
        struct Long : IFSObject, System.IConvertible
    {
        [System.Diagnostics.DebuggerBrowsable(0)]
        internal readonly long m_value;

        /// <summary>
        /// Initializes a new instance of the <see cref="Long"/>
        /// </summary>
        public Long(long value)
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
            return obj is Long l &&
                  m_value == l.m_value ? Boolean.True : Boolean.False;
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

        #region Convertible
        ///<inheritdoc/>
        public System.TypeCode GetTypeCode()
        {
            return System.TypeCode.Int64;
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
            return m_value;
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


        public static implicit operator Long(long value) => new Long(value);

        public static implicit operator Long(Byte value) => new Long(value.m_value);

        public static implicit operator Long(Short value) => new Long(value.m_value);

        public static implicit operator Long(Char value) => new Long(value.m_value);

        public static implicit operator Long(Integer value) => new Long(value.m_value);

        public static implicit operator long(Long value) => value.m_value;

        public static implicit operator double(Long integer) => integer.m_value;

        public static implicit operator float(Long integer) => integer.m_value;

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
            return new Long(value.m_value + 1);
        }
    }
}
