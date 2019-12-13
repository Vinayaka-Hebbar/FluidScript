namespace FluidScript.Dynamic
{
    internal sealed class LocalScope
    {
        private readonly System.Collections.Generic.List<LocalVariable> LocalVariables;
        internal LocalContext Current;

        public LocalScope()
        {
            LocalVariables = new System.Collections.Generic.List<LocalVariable>();
            Current = new LocalContext(this);
        }

        /// <summary>
        /// Getter and Setter of variables
        /// </summary>
        public object this[string name]
        {
            get => Current.Retrieve(name);
            set => CreateOrModify(name, value);
        }

        internal LocalVariable Find(string name)
        {
            return LocalVariables.Find(v => v.Equals(name));
        }

        public LocalVariable Create(string name, System.Type type)
        {
            var local = new LocalVariable(name, LocalVariables.Count, type);
            LocalVariables.Add(local);
            return local;
        }

        public LocalVariable Create(string name, System.Type type, object value)
        {
            var local = new LocalVariable(name, LocalVariables.Count, type);
            LocalVariables.Add(local);
            Current.Create(local, value);
            return local;
        }

        internal void CreateOrModify(string name, object value)
        {
            var variable = Find(name);
            if (variable.Equals(LocalVariable.Empty))
            {
                variable = Create(name, value.GetType());
            }
            Current.Modify(variable, value);
        }

        internal LocalContext CreateContext()
        {
            return new LocalContext(this, Current);
        }
    }
}
