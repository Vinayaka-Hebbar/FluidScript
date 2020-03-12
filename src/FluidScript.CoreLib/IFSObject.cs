namespace FluidScript
{
    /// <summary>
    /// Supports all classes in the FluidScipt
    /// </summary>
    public interface IFSObject
    {
        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        [Runtime.Register("toString")]
        String ToString();
        /// <summary>
        /// Determines whether the specified <see cref="IFSObject"/> is equal to the current <see cref="IFSObject"/>.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>
        /// true if the specified System.Object is equal to the current System.Object; otherwise,
        /// false.
        /// </returns>
        [Runtime.Register("equals")]
        Boolean Equals(object obj);
        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>A hash code for the current <see cref="IFSObject"/>.</returns>
        [Runtime.Register("hashCode")]
        Integer GetHashCode();
    }
}
