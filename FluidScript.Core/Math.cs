namespace FluidScript
{
    public sealed class Math : FSObject
    {
        [Runtime.Register("pow")]
        public Double Pow(Double x, Double y)
        {
            return new Double(System.Math.Pow(x.m_value, y.m_value));
        }
    }
}
