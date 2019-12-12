namespace FluidScript
{
    public sealed class Math : FSObject
    {
        [Runtime.Register("pow")]
        public static Double Pow(Double x, Double y)
        {
            return new Double(System.Math.Pow(x.m_value, y.m_value));
        }

        [Runtime.Register("sqrt")]
        public static Double Sqrt(Double d)
        {
            return new Double(System.Math.Sqrt(d.m_value));
        }
    }
}
