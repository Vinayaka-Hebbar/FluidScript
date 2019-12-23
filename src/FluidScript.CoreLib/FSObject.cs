namespace FluidScript
{
    /// <inheritdoc/>
    public class FSObject : IFSObject
    {
        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj is IFSObject)
                return __Equals((IFSObject)obj).m_value;
            return base.Equals(obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return ((IFSObject)this).HashCode().m_value;
        }

        /// <inheritdoc/>
        [Runtime.Register("hashCode")]
        public virtual Integer HashCode()
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
        public virtual Boolean __Equals(IFSObject obj)
        {
            return ReferenceEquals(this, obj);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return __ToString().m_value;
        }

        /// <summary>
        /// Equal Overload
        /// </summary>
        public static Boolean operator ==(FSObject left, FSObject right)
        {
            return left.__Equals(right);
        }

        /// <summary>
        /// Not Equal Overload
        /// </summary>
        public static Boolean operator !=(FSObject left, FSObject right)
        {
            return left.__Equals(right).m_value ? Boolean.False : Boolean.True;
        }
    }
}
