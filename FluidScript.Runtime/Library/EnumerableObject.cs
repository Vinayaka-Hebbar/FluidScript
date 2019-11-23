using System.Collections;

namespace FluidScript.Library
{
#if Runtime
    public sealed class EnumerableObject : RuntimeObject, IEnumerable
    {
        public readonly IEnumerable Enumerable;

        public EnumerableObject(IEnumerable enumerable)
        {
            Enumerable = enumerable;
        }

        public IEnumerator GetEnumerator()
        {
            return Enumerable.GetEnumerator();
        }
    }
#endif
}
