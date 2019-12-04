namespace FluidScript
{
    [Runtime.Register(nameof(Float))]
    public sealed class Float : FSObject
    {
        internal readonly float _value;

        public Float(float value)
        {
            _value = value;
        }

        [Runtime.Register("toString")]
        public override String __ToString()
        {
            return _value.ToString();
        }

        public static implicit operator Float(Integer value) => new Float(value._value);

        public static implicit operator Float(float value) => new Float(value);

        public static implicit operator float(Float value) => value._value;
    }
}
