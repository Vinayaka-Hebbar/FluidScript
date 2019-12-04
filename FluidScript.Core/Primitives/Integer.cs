namespace FluidScript
{
    [Runtime.Register(nameof(Integer))]
    public sealed class Integer : FSObject
    {
        internal readonly int _value;

        public Integer(int value)
        {
            _value = value;
        }

        [Runtime.Register("toString")]
        public override String __ToString()
        {
            return _value.ToString();
        }

        [Runtime.Register("equals")]
        public override Boolean Equals(IFSObject other)
        {
            return other is Integer integer &&
                  _value == integer._value;
        }

        public override bool Equals(object other)
        {
            return other is Integer integer &&
                  _value == integer._value;
        }

        [Runtime.Register("hashCode")]
        public override Integer HashCode()
        {
            return _value.GetHashCode();
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public static implicit operator Integer(int value) => new Integer(value);

        public static implicit operator Integer(Short value) => new Integer(value._value);

        public static implicit operator int(Integer integer) => integer._value;

        public static Integer operator +(Integer left, Integer right)
        {
            return new Integer(left._value + right._value);
        }

        public static Integer operator -(Integer left, Integer right)
        {
            return new Integer(left._value - right._value);
        }

        public static Integer operator *(Integer left, Integer right)
        {
            return new Integer(left._value * right._value);
        }

        public static Integer operator /(Integer left, Integer right)
        {
            return new Integer(left._value / right._value);
        }

        public static Boolean operator >(Integer left, Integer right)
        {
            return new Boolean(left._value > right._value);
        }

        public static Boolean operator <(Integer left, Integer right)
        {
            return new Boolean(left._value < right._value);
        }

        public static Boolean operator >=(Integer left, Integer right)
        {
            return new Boolean(left._value >= right._value);
        }

        public static Boolean operator <=(Integer left, Integer right)
        {
            return new Boolean(left._value <= right._value);
        }

        public static Boolean operator ==(Integer left, Integer right)
        {
            return new Boolean(left._value == right._value);
        }

        public static Boolean operator !=(Integer left, Integer right)
        {
            return new Boolean(left._value != right._value);
        }
    }
}
