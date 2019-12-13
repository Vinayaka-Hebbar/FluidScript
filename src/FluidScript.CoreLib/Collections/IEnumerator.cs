namespace FluidScript.Collections
{
    /// <summary>
    /// Supports a simple iteration over a nongeneric collection.
    /// </summary>
    public interface IEnumerator : IFSObject, System.Collections.IEnumerator
    {
    }

    /// <summary>
    /// Supports a simple iteration over a generic collection.
    /// </summary>
    /// <typeparam name="T">
    /// The type of objects to enumerate.This type parameter is covariant. That is, you
    /// can use either the type you specified or any type that is more derived. For more
    /// information about covariance and contravariance, see Covariance and Contravariance
    /// in Generics.
    /// </typeparam>
    public interface IEnumerator<T> : IFSObject, System.Collections.Generic.IEnumerator<T> where T : IFSObject
    {
    }
}
