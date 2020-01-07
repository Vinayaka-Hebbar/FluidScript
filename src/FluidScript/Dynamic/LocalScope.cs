namespace FluidScript.Dynamic
{
    internal sealed class LocalScope : System.Collections.Generic.IEnumerable<LocalVariable>
    {
        private readonly System.Collections.Generic.List<LocalVariable> LocalVariables;

        internal LocalContext Current;

        internal LocalScope()
        {
            LocalVariables = new System.Collections.Generic.List<LocalVariable>();
            Current = new LocalContext(this);
        }

        internal LocalScope(LocalScope other)
        {
            LocalVariables = new System.Collections.Generic.List<LocalVariable>(other.LocalVariables);
            Current = other.Current;
        }

        /// <summary>
        /// Getter and Setter of variables
        /// </summary>
        public object this[string name]
        {
            get => Current.Find(name);
            set => CreateOrModify(name, value);
        }

        internal int Count() => LocalVariables.Count;

        internal void Clear()
        {
            LocalVariables.Clear();
        }

        internal System.Collections.Generic.ICollection<string> Keys()
        {
            string[] keys = new string[LocalVariables.Count];
            for (int i = 0; i < LocalVariables.Count; i++)
            {
                keys[i] = LocalVariables[i].Name;
            }
            return keys;
        }

        internal System.Collections.Generic.ICollection<object> Values()
        {
            object[] items = new object[LocalVariables.Count];
            var current = Current;
            for (int i = 0; i < LocalVariables.Count; i++)
            {
                items[i] = current.GetValue(LocalVariables[i]);
            }
            return items;
        }

        internal bool Contains(string name)
        {
            return LocalVariables.Exists(v => v.Equals(name));
        }

        internal bool TryGetValue(string name, out LocalVariable variable)
        {
            foreach (var item in LocalVariables)
            {
                if (item.Equals(name))
                {
                    variable = item;
                    return true;
                }
            }
            variable = LocalVariable.Empty;
            return false;
        }

        public LocalVariable Create(string name, System.Type type)
        {
            var local = new LocalVariable(name, LocalVariables.Count, type);
            LocalVariables.Add(local);
            return local;
        }

        public void Create(string name, System.Type type, object value)
        {
            var local = new LocalVariable(name, LocalVariables.Count, type);
            LocalVariables.Add(local);
            Current.Create(local, value);
        }

        internal void CreateOrModify(string name, object value)
        {
            LocalVariable? variable = null;
            foreach (var item in LocalVariables)
            {
                if (item.Equals(name))
                {
                    variable = item;
                    break;
                }
            }
            if (variable.HasValue == false)
            {
                variable = Create(name, value?.GetType());
            }
            Current.Modify(variable.Value, value);
        }

        internal void RemoveAt(int index)
        {
            LocalVariables.RemoveAt(index);
        }

        internal LocalContext CreateContext()
        {
            return new LocalContext(this, Current);
        }

        public System.Collections.Generic.IEnumerator<LocalVariable> GetEnumerator()
        {
            return LocalVariables.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return LocalVariables.GetEnumerator();
        }

        internal bool Remove(LocalVariable item)
        {
            return LocalVariables.Remove(item);
        }

        internal bool Remove(string name)
        {
            int index = -1;
            for (int i = 0; i < LocalVariables.Count; i++)
            {
                if (LocalVariables[i].Equals(name))
                {
                    index = i;
                    break;
                }
            }
            if (index > -1)
            {
                LocalVariables.RemoveAt(index);
                return true;
            }

            return false;
        }
    }
}
