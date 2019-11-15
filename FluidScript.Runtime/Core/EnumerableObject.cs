using FluidScript.Compiler.Metadata;
using System.Collections;

namespace FluidScript.Core
{
#if Runtime
    public sealed class EnumerableObject : ObjectInstance, IEnumerable
    {
        private static Prototype prototype;
        public readonly IEnumerable Enumerable;

        public EnumerableObject(IEnumerable enumerable)
        {
            Enumerable = enumerable;
        }

        public IEnumerator GetEnumerator()
        {
            return Enumerable.GetEnumerator();
        }

        public override Prototype GetPrototype()
        {
            if (prototype == null)
                prototype = Prototype.Create(GetType());
            return prototype;
        }
    }
#endif
}
