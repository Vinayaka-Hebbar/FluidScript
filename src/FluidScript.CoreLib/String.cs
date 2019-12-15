namespace FluidScript
{
    /// <summary>
    /// Represents text as a series of Unicode characters.
    /// </summary>
    [System.Serializable]
    [System.Runtime.InteropServices.ComVisible(true)]
    public sealed class String : FSObject
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

        /// <inheritdoc/>
        [Runtime.Register("toString")]
        public override String __ToString()
        {
            return m_value.ToString();
        }

        /// <inheritdoc/>
        [Runtime.Register("hashCode")]
        public override Integer HashCode()
        {
            return m_value.GetHashCode();
        }

        /// <inheritdoc/>
        [Runtime.Register("equals")]
        public override Boolean __Equals(IFSObject other)
        {
            return other is String s &&
                  m_value == s.m_value ? Boolean.True : Boolean.False;
        }

        /// <inheritdoc/>
        public override bool Equals(object other)
        {
            return other is String s &&
                  m_value == s.m_value;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return m_value.GetHashCode();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return m_value;
        }

        /// <summary>
        /// Implict conversion from <see cref="string"/> to <see cref="String"/>
        /// </summary>
        public static implicit operator String(string value) => new String(value);


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
