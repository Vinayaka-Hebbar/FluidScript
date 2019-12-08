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

        public Integer this[Integer index]
        {
            get => _value[index];
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

        public override bool Equals(object obj)
        {
            return obj is String @string &&
                   _value == @string._value;
        }

        [Runtime.Register("equals")]
        public override Boolean __Equals(IFSObject obj)
        {
            return obj is String @string &&
                   _value == @string._value;
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        [Runtime.Register("hashCode")]
        public override Integer HashCode()
        {
            return _value.GetHashCode();
        }

        public static implicit operator String(string value) => new String(value);

        public static Boolean operator ==(String left, String right)
        {
            return left._value.Equals(right._value);
        }

        public static Boolean operator !=(String left, String right)
        {
            return left._value.Equals(right._value) == false;
        }

    }
}
