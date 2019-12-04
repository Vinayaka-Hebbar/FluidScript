namespace FluidScript
{
    [Runtime.Register(nameof(Byte))]
    public sealed class Byte : FSObject
    {
        internal readonly sbyte _value;

        public Byte(sbyte value)
        {
            _value = value;
        }
        
        [Runtime.Register("toString")]
        public override String __ToString()
        {
            return _value.ToString();
        }

        public static implicit operator Byte(sbyte value) => new Byte(value);

        public static implicit operator sbyte(Byte value) => value._value;
    }
}
