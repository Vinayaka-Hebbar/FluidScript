using System.Collections.Generic;

namespace FluidScript.Runtime
{
    public interface IDynamicInvocable : IRuntimeMetadata
    {
        Any SafeSetValue(Any value, string name, System.Type type);
        Any SafeGetValue(string name);
        Any Invoke(string name, params Any[] argments);
    }

    public interface IRuntimeMetadata
    {
        bool GetOrCreateBinder(string name, object value, System.Type type, out IMemberBinder binder);
        bool TryGetBinder(string name, out IMemberBinder binder);
        ICollection<string> Keys { get; }
    }
}
