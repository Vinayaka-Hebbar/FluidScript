namespace FluidScript
{
    /// <summary>
    /// Represents a Boolean value.
    /// </summary>
    [System.Serializable]
    [Runtime.Register(nameof(Boolean))]
    [System.Runtime.InteropServices.ComVisible(true)]
    public
#if LATEST_VS
        readonly
#endif
        struct Boolean : IFSObject, System.IConvertible, Runtime.IValueBox<bool>
    {
        /// <summary>
        /// True
        /// </summary>
        public static readonly Boolean True = new Boolean(true);

        /// <summary>
        /// False
        /// </summary>
        public static readonly Boolean False = new Boolean(false);

        [System.Diagnostics.DebuggerBrowsable(0)]
        internal readonly bool m_value;

        /// <summary>
        /// Init New <see cref="Boolean"/> instance
        /// </summary>
        /// <param name="value"></param>
        public Boolean(bool value)
        {
            m_value = value;
        }

        /// <inheritdoc/>
        [Runtime.Register("hashCode")]
        Integer IFSObject.GetHashCode()
        {
            return m_value.GetHashCode();
        }

        /// <inheritdoc/>
        [Runtime.Register("toString")]
        String IFSObject.ToString()
        {
            return m_value.ToString();
        }

        /// <inheritdoc/>
        [Runtime.Register("equals")]
        Boolean IFSObject.Equals(object obj)
        {
            return obj is Boolean b &&
                  m_value == b.m_value ? True : False;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj is Boolean boolean &&
                  m_value == boolean.m_value;
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="obj"> The object to compare with the current instance.</param>
        /// <returns>true if obj and this instance are the same type and represent the same value;
        ///  otherwise, false.</returns>
        public Boolean Equals(Boolean obj)
        {
            return m_value == obj.m_value? True : False;
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
            return System.TypeCode.Boolean;
        }

        string System.IConvertible.ToString(System.IFormatProvider provider)
        {
            return System.Convert.ToString(provider);
        }

        /// <internalonly/>
        bool System.IConvertible.ToBoolean(System.IFormatProvider provider)
        {
            return m_value;
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
        /// Implicit convert from <see cref="bool"/> to <see cref="Boolean"/>
        /// </summary>
        public static implicit operator Boolean(bool value) => value ? True : False;

        /// <summary>
        /// Implicit convert from <see cref="Boolean"/> to <see cref="bool"/>
        /// </summary>
        public static implicit operator bool(Boolean value) => value.m_value;

        /// <summary>
        /// op_Equality implementation
        /// </summary>
        public static Boolean operator ==(Boolean left, Boolean right)
        {
            return left.m_value == right.m_value ? True : False;
        }

        /// <summary>
        /// op_Inequality implementation
        /// </summary>
        public static Boolean operator !=(Boolean left, Boolean right)
        {
            return left.m_value != right.m_value ? True : False;
        }

        /// <summary>
        /// op_LogicalNot implementation
        /// </summary>
        public static Boolean operator !(Boolean value)
        {
            return !value.m_value ? True : False;
        }

        internal static Boolean OpLogicalAnd(Boolean left, Boolean right)
        {
            return left.m_value && right.m_value ? True : False;
        }

        internal static Boolean OpLogicalOr(Boolean left, Boolean right)
        {
            return left.m_value && right.m_value ? True : False;
        }
    }
}
