using System;
using System.Runtime.InteropServices;

namespace FluidScript
{
    [Runtime.Register(nameof(Date))]
    [StructLayout(LayoutKind.Auto)]
    [Serializable]
    public struct Date : IFSObject
    {
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

        [Runtime.Register("now")]
        public static Date Now()
        {
            return new Date(DateTime.Now);
        }

        [Runtime.Register("equals")]
        Boolean IFSObject.Equals(object obj)
        {
            return obj is Date date ? date.m_date == m_date : false;
        }

        [Runtime.Register("toString")]
        Integer IFSObject.GetHashCode()
        {
            return GetHashCode();
        }

        [Runtime.Register("toString")]
        String IFSObject.ToString()
        {
            return m_date.ToString();
        }

        [Runtime.Register("toString")]
        public String ToString(String format)
        {
            return m_date.ToString(format.m_value);
        }

        public override int GetHashCode()
        {
            return m_date.GetHashCode();
        }

    }
}
