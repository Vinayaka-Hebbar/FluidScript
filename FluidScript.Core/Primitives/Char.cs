namespace FluidScript
{
    [Runtime.Register(nameof(Char))]
    public sealed class Char : FSObject
    {
        internal readonly char _value;

        private Char(char value)
        {
            _value = value;
        }

        [Runtime.Register("toString")]
        public override String __ToString()
        {
            return _value.ToString();
        }

        public static implicit operator Char(char value) => new Char(value);

        public static implicit operator char(Char value) => value._value;
    }
}
