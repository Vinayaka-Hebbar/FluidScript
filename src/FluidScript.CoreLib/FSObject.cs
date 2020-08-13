namespace FluidScript
{
    /// <inheritdoc/>
    [Runtime.Register(nameof(System.Object))]
    public class FSObject : IFSObject
    {
        /// <inheritdoc/>
        [Runtime.Register("hashCode")]
        public virtual Integer HashCode()
        {
            return base.GetHashCode();
        }

        /// <inheritdoc/>
        [Runtime.Register("toString")]
        public virtual String StringValue()
        {
            return base.ToString();
        }

        /// <inheritdoc/>
        [Runtime.Register("equals")]
        public virtual Boolean Equals(Any obj)
        {
            return Equals(obj);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString() => StringValue().m_value;

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
            return left.Equals(right);
        }

        [Runtime.Register("isEquals")]
        public static Boolean IsEquals(object arg1, object arg2)
        {
            return Equals(arg1, arg2);
        }
    }
}
