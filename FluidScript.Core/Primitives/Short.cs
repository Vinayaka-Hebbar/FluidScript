namespace FluidScript
{
    [Runtime.Register(nameof(Short))]
    public sealed class Short : FSObject
    {
        internal readonly short _value;

        public Short(short value)
        {
            _value = value;
        }

        [Runtime.Register("toString")]
        public override String __ToString()
        {
            return _value.ToString();
        }

        public static implicit operator Short(short value) => new Short(value);

        public static implicit operator short(Short value) => value._value;
    }
}
