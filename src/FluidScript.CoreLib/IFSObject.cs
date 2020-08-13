namespace FluidScript
{
    /// <summary>
    /// Supports all classes in the FluidScipt
    /// </summary>
    [Runtime.Register("any")]
    public interface IFSObject
    {
        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        [Runtime.Register("toString")]
        String StringValue();

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>A hash code for the current <see cref="IFSObject"/>.</returns>
        [Runtime.Register("hashCode")]
        Integer HashCode();
    }
}
