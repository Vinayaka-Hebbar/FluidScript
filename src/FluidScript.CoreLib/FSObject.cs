namespace FluidScript
{
    /// <inheritdoc/>
    public class FSObject : IFSObject
    {
        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj is IFSObject)
                return Equals((IFSObject)obj).m_value;
            return base.Equals(obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return ((IFSObject)this).__HashCode().m_value;
        }

        /// <inheritdoc/>
        [Runtime.Register("hashCode")]
        public virtual Integer __HashCode()
        {
            return base.GetHashCode();
        }

        /// <inheritdoc/>
        [Runtime.Register("toString")]
        public virtual String __ToString()
        {
            return base.ToString();
        }

        /// <inheritdoc/>
        [Runtime.Register("equals")]
        public virtual Boolean Equals(IFSObject obj)
        {
            return ReferenceEquals(this, obj);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return __ToString().m_value;
        }

        /// <summary>
        /// Equal Implementation
        /// </summary>
        public static Boolean operator ==(FSObject left, FSObject right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Not Equal Implementation
        /// </summary>
        public static Boolean operator !=(FSObject left, FSObject right)
        {
            return left.Equals(right).m_value ? Boolean.False : Boolean.True;
        }

        [Runtime.Register("isEquals")]
        public static Boolean IsEquals(object arg1, object arg2)
        {
            return Equals(arg1, arg2);
        }
    }
}
