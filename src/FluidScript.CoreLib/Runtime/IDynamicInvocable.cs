using System.Collections.Generic;

namespace FluidScript.Runtime
{
    public interface IDynamicInvocable : IRuntimeMetadata
    {
        /// <summary>
        /// Safe set value for dynamic property
        /// </summary>
        /// <param name="value">First argument from right side</param>
        /// <param name="name">name of member</param>
        /// <returns>Updated value</returns>
        Any SafeSetValue(Any value, string name);
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
