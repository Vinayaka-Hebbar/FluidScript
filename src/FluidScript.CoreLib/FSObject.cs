namespace FluidScript
{
    /// <inheritdoc/>
    public class FSObject : IFSObject
    {

        /// <inheritdoc/>
        [Runtime.Register("hashCode")]
        Integer IFSObject.GetHashCode()
        {
            return base.GetHashCode();
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <inheritdoc/>
        [Runtime.Register("toString")]
        String IFSObject.ToString()
        {
            return ToString();
        }
        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        /// <inheritdoc/>
        [Runtime.Register("equals")]
        Boolean IFSObject.Equals(object obj)
        {
            return Equals(obj);
        }

        /// <summary>
        /// Equal Implementation
        /// </summary>
        public static Boolean operator ==(FSObject left, FSObject right)
        {
            return ((IFSObject)left).Equals(right);
        }

        /// <summary>
        /// Not Equal Implementation
        /// </summary>
        public static Boolean operator !=(FSObject left, FSObject right)
        {
            return ((IFSObject)left).Equals(right);
        }

        [Runtime.Register("isEquals")]
        public static Boolean IsEquals(object arg1, object arg2)
        {
            return Equals(arg1, arg2);
        }
    }
}
