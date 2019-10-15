namespace FluidScript.Core
{
    public struct TypeNameContext : IInvocationContext
    {
        public readonly string Name;

        public TypeNameContext(string name)
        {
            Name = name;
        }

        public bool CanInvoke => false;

        public Object Invoke(Object target)
        {
            throw new System.NotImplementedException();
        }

        internal System.Type Type()
        {
            return System.Type.GetType(Name);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
