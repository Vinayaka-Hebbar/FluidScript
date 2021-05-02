using System.Collections.Generic;

namespace FluidScript.Runtime
{
    public interface ILocalVariables :
        IDictionary<string, object>, System.Runtime.CompilerServices.IRuntimeVariables, IEnumerable<KeyValuePair<string, object>>
    {
        LocalVariable DeclareVariable(string name, System.Type type, object value);
        LocalVariable DeclareVariable<T>(string name, T value = default(T));
        bool TryFindVariable(string key, out LocalVariable variable);
    }
}
