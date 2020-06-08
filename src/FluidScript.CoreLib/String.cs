namespace FluidScript
{
    /// <summary>
    /// Represents text as a series of Unicode characters.
    /// </summary>
    [System.Serializable]
    [Runtime.Register(nameof(String))]
    [System.Runtime.InteropServices.ComVisible(true)]
    public sealed class String : FSObject, System.IConvertible
    {
        [System.Diagnostics.DebuggerBrowsable(0)]
        internal readonly string m_value;

        /// <summary>
        /// Initializes a new instance of the <see cref="String"/>
        /// </summary>
        public String(string value)
        {
            m_value = value;
        }

        /// <summary>
        /// Char Unicode at <paramref name="index"/>
        /// </summary>
        public Char this[Integer index]
        {
            get
            {
                return index < m_value.Length ? (Char)m_value[index] : Char.MinValue;
            }
        }

        [Runtime.Register("length")]
        public Integer Length
        {
            get
            {
                return new Integer(m_value.Length);
            }
        }

        [Runtime.Register("indexOf")]
        public Integer IndexOf(String value) => m_value.IndexOf(value.m_value);

        [Runtime.Register("replace")]
        public String Replace(String oldValue, String newValue) => m_value.Replace(oldValue.m_value, newValue?.m_value);

        [Runtime.Register("split")]
        public String[] Split(String separator)
        {
            unsafe
            {
                var value = m_value;
                string m_seperator = separator.m_value;
                int splitLength;
                int count = 0;
                int[] positions = new int[value.Length];
                fixed (char* values = value)
                {
                    if (m_seperator == null || m_seperator.Length == 0)
                    {
                        return new String[1] { value };
                    }
                    else
                    {
                        splitLength = m_seperator.Length;
                        for (int i = 0; i < value.Length; i++)
                        {
                            if (values[i] == m_seperator[0] && splitLength < value.Length - i)
                            {
                                if (splitLength == 1 || string.CompareOrdinal(value, i, m_seperator, 0, splitLength) == 0)
                                {
                                    positions[count++] = i;
                                }
                            }
                        }

                    }

                    String[] result = new String[count + 1];
                    var length = value.Length;
                    int pos = 0;
                    int index;
                    for (index = 0; index < count && pos < length; index++)
                    {
                        var splitPos = positions[index];
                        result[index] = value.Substring(pos, splitPos - pos);
                        pos = splitPos + splitLength;
                    }
                    if (pos < length && count >= 0)
                    {
                        result[index] = value.Substring(pos);
                    }
                    return result;
                }
            }
        }

        [Runtime.Register("subString")]
        public String SubString(int startIndex, int length) => m_value.Substring(startIndex, length);

        /// <inheritdoc/>
        public override string ToString()
        {
            return m_value;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return m_value.GetHashCode();
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="obj"> The object to compare with the current instance.</param>
        /// <returns>true if obj and this instance are the same type and represent the same value;
        ///  otherwise, false.</returns>
        [Runtime.Register("equals")]
        public Boolean Equals(String obj)
        {
            return m_value == obj.m_value ? Boolean.True : Boolean.False;
        }

        /// <inheritdoc/>
        public override bool Equals(object other)
        {
            return other is String s &&
                  m_value == s.m_value;
        }

        ///<inheritdoc/>
        public System.TypeCode GetTypeCode()
        {
            return System.TypeCode.String;
        }

        /// <internalonly/>
        bool System.IConvertible.ToBoolean(System.IFormatProvider provider)
        {
            return System.Convert.ToBoolean(m_value, provider);
        }

        /// <internalonly/>
        char System.IConvertible.ToChar(System.IFormatProvider provider)
        {
            return System.Convert.ToChar(m_value, provider);
        }

        /// <internalonly/>
        sbyte System.IConvertible.ToSByte(System.IFormatProvider provider)
        {
            return System.Convert.ToSByte(m_value, provider);
        }

        /// <internalonly/>
        byte System.IConvertible.ToByte(System.IFormatProvider provider)
        {
            return System.Convert.ToByte(m_value, provider);
        }

        /// <internalonly/>
        short System.IConvertible.ToInt16(System.IFormatProvider provider)
        {
            return System.Convert.ToInt16(m_value, provider);
        }

        /// <internalonly/>
        ushort System.IConvertible.ToUInt16(System.IFormatProvider provider)
        {
            return System.Convert.ToUInt16(m_value, provider);
        }

        /// <internalonly/>
        int System.IConvertible.ToInt32(System.IFormatProvider provider)
        {
            return System.Convert.ToInt32(m_value, provider);
        }

        /// <internalonly/>
        uint System.IConvertible.ToUInt32(System.IFormatProvider provider)
        {
            return System.Convert.ToUInt32(m_value, provider);
        }

        /// <internalonly/>
        long System.IConvertible.ToInt64(System.IFormatProvider provider)
        {
            return System.Convert.ToInt64(m_value, provider);
        }

        /// <internalonly/>
        ulong System.IConvertible.ToUInt64(System.IFormatProvider provider)
        {
            return System.Convert.ToUInt64(m_value, provider);
        }

        /// <internalonly/>
        float System.IConvertible.ToSingle(System.IFormatProvider provider)
        {
            return System.Convert.ToSingle(m_value, provider);
        }

        /// <internalonly/>
        double System.IConvertible.ToDouble(System.IFormatProvider provider)
        {
            return System.Convert.ToDouble(m_value, provider);
        }

        /// <internalonly/>
        decimal System.IConvertible.ToDecimal(System.IFormatProvider provider)
        {
            return System.Convert.ToDecimal(m_value, provider);
        }

        /// <internalonly/>
        System.DateTime System.IConvertible.ToDateTime(System.IFormatProvider provider)
        {
            return System.Convert.ToDateTime(m_value, provider);
        }

        /// <internalonly/>
        object System.IConvertible.ToType(System.Type type, System.IFormatProvider provider)
        {
            return m_value;
        }

        string System.IConvertible.ToString(System.IFormatProvider provider)
        {
            return m_value;
        }

        /// <summary>
        /// Implict conversion from <see cref="string"/> to <see cref="String"/>
        /// </summary>
        public static implicit operator String(string value) => new String(value);
        /// <summary>
        /// Implict conversion from <see cref="String"/> to <see cref="string"/>
        /// </summary>
        public static implicit operator string(String value) => value.m_value;


        /// <summary>
        /// op_Addition overload
        /// </summary>
        public static String operator +(String left, String right)
        {
            return new String(string.Concat(left.m_value, right.m_value));
        }

        /// <summary>
        /// op_Equality overload
        /// </summary>
        public static Boolean operator ==(String left, String right)
        {
            return left.m_value.Equals(right.m_value) ? Boolean.True : Boolean.False;
        }

        /// <summary>
        /// op_InEquality overload
        /// </summary>
        public static Boolean operator !=(String left, String right)
        {
            return left.m_value.Equals(right.m_value) == false ? Boolean.True : Boolean.False;
        }

    }
}
