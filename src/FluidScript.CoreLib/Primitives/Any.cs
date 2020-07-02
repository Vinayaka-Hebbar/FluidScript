using System.Runtime.CompilerServices;

namespace FluidScript
{
    public struct Any
    {
        object m_value;

        public Any(object value)
        {
            m_value = value;
        }

        [SpecialName]
        public static Any op_Implicit(object value)
        {
            return new Any(value);
        }
    }
}
