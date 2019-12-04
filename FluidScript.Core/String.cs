namespace FluidScript
{
    [Runtime.Register(nameof(String))]
    public sealed class String : FSObject
    {
        internal readonly string _value;

        public String(string value)
        {
            _value = value;
        }

        public static String operator +(String left, String right)
        {
            return new String(string.Concat(left._value, right._value));
        }

        [Runtime.Register("toString")]
        public override String __ToString()
        {
            return this;
        }

        public override string ToString()
        {
            return _value;
        }

        public static implicit operator String(string value) => new String(value);

    }
}
