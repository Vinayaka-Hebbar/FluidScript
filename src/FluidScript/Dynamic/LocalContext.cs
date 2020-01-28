using System.Collections.Generic;

namespace FluidScript.Dynamic
{
    internal sealed class LocalContext : Dictionary<LocalVariable, object>, System.IDisposable
    {
        private readonly LocalInstance _obj;

        internal readonly LocalContext Parent;

        internal LocalContext(LocalInstance scope)
        {
            _obj = scope;
        }

        internal LocalContext(LocalInstance obj, LocalContext parent)
        {
            _obj = obj;
            _obj.Current = this;
            Parent = parent;
        }

        internal void Modify(LocalVariable variable, object value)
        {
            if (ContainsKey(variable) == false && Parent != null)
            {
                Parent.Modify(variable, value);
                return;
            }
            this[variable] = value;
        }

        internal object Find(string name)
        {
            if (_obj.TryGetMember(name, out LocalVariable variable))
            {
                if (TryGetValue(variable, out object store))
                    return store;
                if (Parent != null)
                {
                    return Parent.Find(name);
                }
            }
            return null;
        }

        public void Dispose()
        {
            foreach (var item in System.Linq.Enumerable.Reverse(Keys))
            {
                _obj.Remove(item);
            }
            _obj.Current = Parent;
        }

        internal bool TryFind(LocalVariable item, out object value)
        {
            if (TryGetValue(item, out value))
                return true;
            if (Parent != null)
            {
                return Parent.TryFind(item, out value);
            }
            value = null;
            return false;
        }

        internal object GetValue(LocalVariable item)
        {
            if (TryGetValue(item, out object value) == false && Parent != null)
            {
                return Parent.GetValue(item);
            }
            return value;
        }

        internal void CreateGlobal(LocalVariable variable, object value)
        {
            if (Parent != null)
                Parent.CreateGlobal(variable, value);
            else
                this[variable] = value;
        }
    }

    public
#if LATEST_VS
        readonly
#endif
        struct LocalVariable : IFSObject
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

        public bool IsEmpty => Index > 0;

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

        [Runtime.Register("hashCode")]
        public Integer HashCode()
        {
            return new Integer(((hcf + Name.GetHashCode()) * hcs) + Index);
        }

        public override string ToString() => string.Concat(Name, ":", Type.Name);

        [Runtime.Register("equals")]
        public Boolean __Equals(IFSObject obj)
        {
            return Equals(obj) ? Boolean.True : Boolean.False;
        }

        [Runtime.Register("toString")]
        public String __ToString()
        {
            return new String(string.Concat(Name, ":", Type.Name));
        }
    }
}