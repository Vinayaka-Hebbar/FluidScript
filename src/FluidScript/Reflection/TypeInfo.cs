namespace FluidScript.Reflection
{
    public interface ITypeInfo
    {
        string Name { get; }
        TypeName TypeName { get; }
        string FullName { get; }
        ITypeInfo ElementType { get; }
        bool IsArray();
        ITypeInfo MakeArrayType();
        System.Type ResolvedType(Emit.ITypeProvider generator);
    }

    public
#if LATEST_VS
        readonly
#endif
        struct TypeInfo : ITypeInfo
    {
        public readonly static TypeInfo Any = new TypeInfo("any");
        public readonly static TypeInfo Void = new TypeInfo("void");

        public string FullName { get; }

        public string Name
        {
            get
            {
                return TypeName.Name;
            }
        }

        public ITypeInfo ElementType => this;

        public TypeInfo(string name)
        {
            FullName = name;
        }

        public TypeName TypeName
        {
            get
            {
                return FullName;
            }
        }

        public System.Type ResolvedType(Emit.ITypeProvider generator)
        {
            return generator.GetType(TypeName);
        }

        public bool IsArray()
        {
            return false;
        }

        public override string ToString()
        {
            return FullName;
        }

        public ITypeInfo MakeArrayType()
        {
            return new ArrayTypeInfo(FullName, this, 1);
        }
    }

    public
#if LATEST_VS
        readonly
#endif
        struct ArrayTypeInfo : ITypeInfo
    {
        public ITypeInfo ElementType { get; }

        public string FullName { get; }

        public string Name
        {
            get
            {
                return TypeName.Name;
            }
        }

        private readonly int arrayRank;

        public ArrayTypeInfo(string name, ITypeInfo elementType, int arrayRank)
        {
            FullName = string.Concat(name, "[]");
            ElementType = elementType;
            this.arrayRank = arrayRank;
        }

        public TypeName TypeName
        {
            get
            {
                return FullName;
            }
        }

        public System.Type ResolvedType(Emit.ITypeProvider generator)
        {
            var type = ElementType.ResolvedType(generator);
            for (int rank = 0; rank < arrayRank; rank++)
            {
                type = type.MakeArrayType();
            }
            return type;
        }

        public bool IsArray()
        {
            return true;
        }

        public override string ToString()
        {
            return FullName;
        }

        public ITypeInfo MakeArrayType()
        {
            return new ArrayTypeInfo(FullName, ElementType, arrayRank + 1);
        }
    }
}
