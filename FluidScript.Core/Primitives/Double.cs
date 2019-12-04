namespace FluidScript
{
    [Runtime.Register(nameof(Double))]
    public sealed class Double : FSObject
    {
        private readonly double _value;

        public Double(double value)
        {
            _value = value;
        }

        [Runtime.Register("toString")]
        public override String __ToString()
        {
            return _value.ToString();
        }

        public static implicit operator Double(int value) => new Double(value);

        public static implicit operator Double(Integer integer) => new Double(integer._value);

        public static implicit operator Double(Float single) => new Double(single._value);

        public static implicit operator double(Double integer) => integer._value;
    }
}
