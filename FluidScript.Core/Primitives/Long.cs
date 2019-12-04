namespace FluidScript
{
    [Runtime.Register(nameof(Long))]
    public sealed class Long : FSObject
    {
        internal readonly long _value;

        public Long(long value)
        {
            _value = value;
        }

        [Runtime.Register("toString")]
        public override String __ToString()
        {
            return _value.ToString();
        }

        public static implicit operator Long(long value) => new Long(value);

        public static implicit operator long(Long value) => value._value;
    }
}
