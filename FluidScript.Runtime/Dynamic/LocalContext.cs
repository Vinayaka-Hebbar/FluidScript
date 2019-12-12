using System.Collections.Generic;

namespace FluidScript.Dynamic
{
    internal sealed class LocalContext
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
            _parent = parent;
        }

        internal object Retrieve(string name)
        {
            var variable = _scope.Find(name);
            if (variable.Equals(LocalVariable.Empty) == false && Instances.TryGetValue(variable, out object store))
                return store;
            if (_parent == null)
            {
                return null;
            }
            return _parent.Retrieve(name);
        }

        internal void CreateOrModify(string name, object value)
        {
            var variable = _scope.Find(name);
            if (variable.Equals(LocalVariable.Empty))
            {
                variable = _scope.Create(name, value.GetType());
            }
            Instances[variable] = value;
        }

        internal void Modify(LocalVariable variable, object value)
        {
            Instances[variable] = value;
        }

        internal LocalVariable? Find(string name, out object store)
        {
            var variable = _scope.Find(name);
            if (variable.Equals(LocalVariable.Empty) == false && Instances.TryGetValue(variable, out store))
                return variable;
            if (_parent == null)
            {
                store = null;
                return variable;
            }
            return _parent.Find(name, out store);
        }

        internal void Create(string name, System.Type type, object value)
        {
            var variable = _scope.Find(name);
            if (variable.Equals(LocalVariable.Empty))
            {
                variable = _scope.Create(name, type);
            }
            Instances[variable] = value;
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