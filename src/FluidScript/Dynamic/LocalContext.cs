using System.Collections.Generic;

namespace FluidScript.Dynamic
{
    internal sealed class LocalContext : System.IDisposable
    {
        private readonly IDictionary<LocalVariable, object> _instances = new Dictionary<LocalVariable, object>();

        private readonly LocalScope _scope;

        internal readonly LocalContext Parent;

        internal LocalContext(LocalScope scope)
        {
            _scope = scope;
        }

        internal LocalContext(LocalScope scope, LocalContext parent)
        {
            _scope = scope;
            _scope.Current = this;
            Parent = parent;
        }

        internal void Modify(LocalVariable variable, object value)
        {
            if (_instances.ContainsKey(variable) == false && Parent != null)
            {
                Parent.Modify(variable, value);
                return;
            }
            _instances[variable] = value;
        }

        internal object Find(string name)
        {
            if (_scope.TryGetMember(name, out LocalVariable variable))
            {
                if (_instances.TryGetValue(variable, out object store))
                    return store;
                if (Parent != null)
                {
                    return Parent.Find(name);
                }
            }
            return null;
        }

        internal ICollection<LocalVariable> Variables
        {
            get
            {
                return _instances.Keys;
            }
        }

        internal void Create(LocalVariable variable, object value)
        {
            _instances[variable] = value;
        }

        public void Dispose()
        {
            foreach (var item in System.Linq.Enumerable.Reverse(_instances.Keys))
            {
                _scope.Remove(item);
            }
            _scope.Current = Parent;
        }

        internal bool TryGetValue(LocalVariable item, out object value)
        {
            if (_instances.TryGetValue(item, out value))
                return true;
            if (Parent != null)
            {
                return Parent.TryGetValue(item, out value);
            }
            value = null;
            return false;
        }

        internal object GetValue(LocalVariable item)
        {
            if (_instances.TryGetValue(item, out object value) == false && Parent != null)
            {
                return Parent.GetValue(item);
            }
            return value;
        }
    }

    internal
#if LATEST_VS
        readonly
#endif
        struct LocalVariable
    {
        private const int hcf = 2063038313;
        private const int hcs = -1521134295;

        internal static readonly LocalVariable Empty = new LocalVariable(null, -1, null);

        internal readonly string Name;
        internal readonly int Index;
        internal readonly System.Type Type;

        public LocalVariable(string name, int index, System.Type type)
        {
            Name = name;
            Index = index;
            Type = type;
        }

        public override bool Equals(object obj)
        {
            if (obj is LocalVariable other)
            {
                if (Name == null && other.Name == null)
                    return true;
                return Name.Equals(other.Name);
            }
            return Name.Equals(obj);
        }

        public override int GetHashCode()
        {
            return ((hcf + Name.GetHashCode()) * hcs) + Index;
        }

        public override string ToString() => string.Concat(Name, ":", Type.Name);
    }
}