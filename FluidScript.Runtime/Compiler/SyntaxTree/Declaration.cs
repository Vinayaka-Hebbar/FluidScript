namespace FluidScript.Compiler.SyntaxTree
{
    public abstract class Declaration
    {
        public readonly string Name;
        public PrimitiveType PrimitiveType;
        private System.Type resolvedType;

        internal Declaration(string name, FluidScript.Compiler.Emit.TypeName typeName)
        {
            Name = name;
            TypeName = typeName;
        }

        protected Declaration(string name)
        {
            Name = name;
        }


        public virtual FluidScript.Compiler.Emit.TypeName TypeName { get; }

        /// <summary>
        /// either return type, field type or property type
        /// </summary>
        public System.Type ResolvedType
        {
            get => resolvedType;
            protected set
            {
                resolvedType = value;
                PrimitiveType = Emit.TypeUtils.ToPrimitive(value);
            }
        }


        protected virtual void TryResolveType(Emit.OptimizationInfo typeProvider)
        {
            var typeName = TypeName;
            if (typeName.FullName != null)
            {
                ResolvedType = typeProvider.GetType(typeName);
            }
        }

    }
}
