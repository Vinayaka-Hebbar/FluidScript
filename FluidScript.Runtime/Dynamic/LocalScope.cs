namespace FluidScript.Dynamic
{
    internal sealed class LocalScope
    {
        private readonly System.Collections.Generic.List<LocalVariable> LocalVariables;

        public LocalScope()
        {
            LocalVariables = new System.Collections.Generic.List<LocalVariable>();
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
    }
}
