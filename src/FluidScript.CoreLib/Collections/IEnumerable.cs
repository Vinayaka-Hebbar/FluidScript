namespace FluidScript.Collections
{
    public interface IEnumerable<out T> : System.Collections.Generic.IEnumerable<T>
    {
        [Runtime.Register("enumerator")]
        IEnumerator Enumerator();
    }
}
