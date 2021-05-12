using FluidScript.Runtime;
using System;
using System.Runtime.InteropServices;

namespace FluidScript
{
    [Register(nameof(Date))]
    [Serializable]
    [StructLayout(LayoutKind.Auto)]
    public readonly struct Date : IFSObject, IFormattable, IConvertible
    {
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private readonly DateTime m_date;

        public Date(Long ticks) 
        {
            m_date = new DateTime(ticks.m_value);
        }

        public Date(Integer year, Integer month, Integer day)
        {
            m_date = new DateTime(year.m_value, month.m_value, day.m_value);
        }

        internal Date(DateTime value)
        {
            m_date = value;
        }

        /// <summary>
        /// Returns a new System.DateTime that adds the specified number of ticks to the
        /// value of this instance.
        /// </summary>
        /// <param name="value">A number of 100-nanosecond ticks. The value parameter can be positive or negative.</param>
        /// <returns>An object whose value is the sum of the date and time represented by this instance
        ///  and the time represented by value.</returns>
        ///  <exception cref="System.ArgumentOutOfRangeException">
        ///  The resulting System.DateTime is less than System.DateTime.MinValue or greater
        ///  than System.DateTime.MaxValue.</exception>
        [Register("addTicks")]
        public Date AddTicks(Long value)
        {
            return new Date(m_date.AddTicks(value.m_value));
        }

        [Register("now")]
        public static Date Now()
        {
            return new Date(DateTime.Now);
        }

        [Register("equals")]
        public Boolean Equals(Date date)
        {
            return date.m_date == m_date;
        }

        [Register("hashCode")]
        public Integer HashCode()
        {
            return GetHashCode();
        }

        [Register("toString")]
        public String StringValue()
        {
            return m_date.ToString();
        }

        [Register("toString")]
        public String ToString(String format)
        {
            return m_date.ToString(format.m_value);
        }

        [Register("parse")]
        public static Date Parse(String s)
        {
            return new Date(DateTime.Parse(s.m_value));
        }

        public override string ToString()
        {
            return m_date.ToString();
        }

        string IFormattable.ToString(string format, IFormatProvider provider)
        {
            return m_date.ToString(format, provider);
        }

        public override int GetHashCode()
        {
            return m_date.GetHashCode();
        }

        #region IConvertible
        TypeCode IConvertible.GetTypeCode()
        {
            return TypeCode.DateTime;
        }

        bool IConvertible.ToBoolean(IFormatProvider provider)
        {
            return ((IConvertible)m_date).ToBoolean(provider);
        }

        byte IConvertible.ToByte(IFormatProvider provider)
        {
            return ((IConvertible)m_date).ToByte(provider);
        }

        char IConvertible.ToChar(IFormatProvider provider)
        {
            return ((IConvertible)m_date).ToChar(provider);
        }

        DateTime IConvertible.ToDateTime(IFormatProvider provider)
        {
            return m_date;
        }

        decimal IConvertible.ToDecimal(IFormatProvider provider)
        {
            return ((IConvertible)m_date).ToDecimal(provider);
        }

        double IConvertible.ToDouble(IFormatProvider provider)
        {
            return ((IConvertible)m_date).ToDouble(provider);
        }

        short IConvertible.ToInt16(IFormatProvider provider)
        {
            return ((IConvertible)m_date).ToInt16(provider);
        }

        int IConvertible.ToInt32(IFormatProvider provider)
        {
            return ((IConvertible)m_date).ToInt32(provider);
        }

        long IConvertible.ToInt64(IFormatProvider provider)
        {
            return ((IConvertible)m_date).ToInt64(provider);
        }

        sbyte IConvertible.ToSByte(IFormatProvider provider)
        {
            return ((IConvertible)m_date).ToSByte(provider);
        }

        float IConvertible.ToSingle(IFormatProvider provider)
        {
            return ((IConvertible)m_date).ToSingle(provider);
        }

        string IConvertible.ToString(IFormatProvider provider)
        {
            return m_date.ToString(provider);
        }

        object IConvertible.ToType(Type conversionType, IFormatProvider provider)
        {
            return ((IConvertible)m_date).ToType(conversionType, provider);
        }

        ushort IConvertible.ToUInt16(IFormatProvider provider)
        {
            return ((IConvertible)m_date).ToUInt16(provider);
        }

        uint IConvertible.ToUInt32(IFormatProvider provider)
        {
            return ((IConvertible)m_date).ToUInt32(provider);
        }

        ulong IConvertible.ToUInt64(IFormatProvider provider)
        {
            return ((IConvertible)m_date).ToUInt64(provider);
        }
        #endregion

        public static implicit operator Date(DateTime dateTime)
        {
            return new Date(dateTime);
        }

        public static implicit operator DateTime(Date date)
        {
            return date.m_date;
        }
    }
}
