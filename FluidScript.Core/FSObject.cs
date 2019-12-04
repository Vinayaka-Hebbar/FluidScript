namespace FluidScript
{
    [Runtime.Register("any")]
    public class FSObject : IFSObject
    {
        public override bool Equals(object obj)
        {
            if (obj is IFSObject)
                return Equals((IFSObject)obj)._value;
            return base.Equals(obj);
        }

        [Runtime.Register("equals")]
        public virtual Boolean Equals(IFSObject obj)
        {
            return ReferenceEquals(this, obj);
        }

        public override int GetHashCode()
        {
            return ((IFSObject)this).HashCode()._value;
        }

        [Runtime.Register("hashCode")]
        public virtual Integer HashCode()
        {
            return base.GetHashCode();
        }

        [Runtime.Register("toString")]
        public virtual String __ToString()
        {
            return base.ToString();
        }

        public override string ToString()
        {
            return __ToString()._value;
        }


    }
}
