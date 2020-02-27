namespace FluidScript.Compiler.Emit
{
    /// <summary>
    /// Represents a local variable in CIL code.
    /// </summary>
    public class ILLocalVariable
    {
        /// <summary>
        /// Gets the zero-based index of the local variable within the method body.
        /// </summary>
        public int Index => UnderlyingLocal.LocalIndex;

        /// <summary>
        /// Gets the type of the local variable.
        /// </summary>
        public System.Type Type => UnderlyingLocal.LocalType;

        /// <summary>
        /// Gets the local variable name, or <c>null</c> if a name was not provided.
        /// </summary>
        public string Name { get; }

        public readonly System.Reflection.Emit.LocalBuilder UnderlyingLocal;

        public ILLocalVariable(System.Reflection.Emit.LocalBuilder local, string name)
        {
            UnderlyingLocal = local;
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}