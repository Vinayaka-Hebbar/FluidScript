namespace FluidScript
{
    [Runtime.Register(nameof(Boolean))]
    public sealed class Boolean : FSObject
    {
        internal readonly bool _value;

        public Boolean(bool value)
        {
            _value = value;
        }

        [Runtime.Register("toString")]
        public override String __ToString()
        {
            return _value.ToString();
        }

        public static implicit operator Boolean(bool value) => new Boolean(value);

        public static implicit operator bool(Boolean value) => value._value;

    }
}
