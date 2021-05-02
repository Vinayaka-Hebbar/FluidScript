using System;
using System.Runtime.InteropServices;

namespace FluidScript
{
    [Runtime.Register(nameof(Date))]
    [Serializable]
    [StructLayout(LayoutKind.Auto)]
    public struct Date : IFSObject, IFormattable
    {
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private DateTime m_date;

        public Date(Long ticks) : this()
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
        [Runtime.Register("addTicks")]
        public Date AddTicks(Long value)
        {
            return new Date(m_date.AddTicks(value.m_value));
        }

        [Runtime.Register("now")]
        public static Date Now()
        {
            return new Date(DateTime.Now);
        }

        [Runtime.Register("equals")]
        public Boolean Equals(Date date)
        {
            return date.m_date == m_date;
        }

        [Runtime.Register("toString")]
        public Integer HashCode()
        {
            return GetHashCode();
        }

        [Runtime.Register("toString")]
        public String StringValue()
        {
            return m_date.ToString();
        }

        [Runtime.Register("toString")]
        public String ToString(String format)
        {
            return m_date.ToString(format.m_value);
        }

        [Runtime.Register("parse")]
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
    }
}
