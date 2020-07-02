using System.Runtime.CompilerServices;

namespace FluidScript
{
    /// <inheritdoc/>
    [Runtime.Register(nameof(System.Object))]
    public class FSObject : IFSObject
    {
        /// <inheritdoc/>
        [Runtime.Register("hashCode")]
        Integer IFSObject.GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <inheritdoc/>
        [Runtime.Register("toString")]
        String IFSObject.ToString()
        {
            return base.ToString();
        }

        /// <inheritdoc/>
        [Runtime.Register("equals")]
        Boolean IFSObject.Equals(object obj)
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

        public override string ToString()
        {
            return ((IFSObject)this).ToString().m_value;
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
