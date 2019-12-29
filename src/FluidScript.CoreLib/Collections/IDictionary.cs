using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluidScript.Collections
{
    public interface IDictionary<TKey,TValue> : IFSObject, ICollection<KeyValuePair<TKey,TValue>>
    {
        [Runtime.Register("contains")]
        int Count { get; }
    }
}
