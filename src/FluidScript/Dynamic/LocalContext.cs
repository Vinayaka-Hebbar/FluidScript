using System;
using System.Collections.Generic;

namespace FluidScript.Dynamic
{
    internal sealed class LocalContext : System.IDisposable
    {
        private readonly IDictionary<LocalVariable, object> Instances = new Dictionary<LocalVariable, object>();

        private readonly LocalScope _scope;

        private readonly LocalContext _parent;

        public LocalContext(LocalScope scope)
        {
            _scope = scope;
        }

        public LocalContext(LocalScope scope, LocalContext parent)
        {
            _scope = scope;
            _scope.Current = this;
            _parent = parent;
        }

        internal void Modify(LocalVariable variable, object value)
        {
            Instances[variable] = value;
        }

        internal object Find(string name)
        {
            if (_scope.TryGetValue(name, out LocalVariable variable))
            {
                if (Instances.TryGetValue(variable, out object store))
                    return store;
                if (_parent != null)
                {
                    return _parent.Find(name);
                }
            }
            return null;
        }

        internal void Create(LocalVariable variable, object value)
        {
            Instances[variable] = value;
        }

        public void Dispose()
        {
            foreach (var item in System.Linq.Enumerable.Reverse(Instances.Keys))
            {
                _scope.RemoveAt(item.Index);
            }
            _scope.Current = _parent;
        }

        internal bool TryGetValue(LocalVariable item, out object value)
        {
            if (Instances.TryGetValue(item, out value))
                return true;
            if (_parent != null)
            {
                return _parent.TryGetValue(item, out value);
            }
            value = null;
            return false;
        }

        internal object GetValue(LocalVariable item)
        {
            if (Instances.TryGetValue(item, out object value) == false && _parent != null)
            {
                return _parent.GetValue(item);
            }
            return value;
        }
    }

    internal readonly struct LocalVariable
    {
        internal static readonly LocalVariable Empty = default;

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
            return Name.GetHashCode();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}