namespace FluidScript.Reflection
{
    public interface ITypeInfo
    {
        string Name { get; }
        TypeName TypeName { get; }
        string FullName { get; }
        RuntimeType RuntimeType { get; }
        ITypeInfo ElementType { get; }
        bool IsArray();
        ITypeInfo MakeArrayType();
        System.Type ResolvedType(Emit.ITypeProvider generator);
    }

    public struct TypeInfo : ITypeInfo
    {
        public readonly static TypeInfo Any = new TypeInfo("any", RuntimeType.Any);
        public readonly static TypeInfo Void = new TypeInfo("void", RuntimeType.Void);

        public string FullName { get; }

        public string Name
        {
            get
            {
                return TypeName.Name;
            }
        }

        public ITypeInfo ElementType => this;

        private RuntimeType runtimeType;

        public RuntimeType RuntimeType
        {
            get
            {
                if (runtimeType == RuntimeType.Undefined)
                {
                    runtimeType = Emit.TypeUtils.GetRuntimeType(this);
                }
                return runtimeType;
            }
        }

        public TypeInfo(string name, RuntimeType runtime = RuntimeType.Undefined)
        {
            FullName = name;
            runtimeType = runtime;
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
            return new ArrayTypeInfo(FullName, this, 1, RuntimeType | RuntimeType.Array);
        }
    }

    public struct ArrayTypeInfo : ITypeInfo
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

        private RuntimeType runtimeType;

        public RuntimeType RuntimeType
        {
            get
            {
                if (runtimeType == RuntimeType.Undefined)
                {
                    runtimeType = Emit.TypeUtils.GetRuntimeType(this);
                }
                return runtimeType;
            }
        }

        public ArrayTypeInfo(string name, ITypeInfo elementType, int arrayRank, RuntimeType runtime = RuntimeType.Undefined)
        {
            FullName = string.Concat(name, "[]");
            ElementType = elementType;
            this.arrayRank = arrayRank;
            runtimeType = runtime;
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
            return new ArrayTypeInfo(FullName, ElementType, arrayRank + 1, RuntimeType);
        }
    }
}
