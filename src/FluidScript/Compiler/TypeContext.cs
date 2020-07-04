using FluidScript.Compiler.Emit;
using System;
using System.Collections.Generic;

namespace FluidScript.Compiler
{
    public class TypeContext : ITypeContext
    {
        static TypeContext defaultContext;
        public static TypeContext Default
        {
            get
            {
                if (defaultContext == null)
                {
                    defaultContext = new TypeContext(null);
                }

                return defaultContext;
            }
        }

        Dictionary<string, Type> importedTypes;
        internal IDictionary<string, Type> ImportedTypes
        {
            get
            {
                if (importedTypes == null)
                {
                    if (parent == null)
                    {
                        importedTypes = new Dictionary<string, Type>(TypeProvider.Inbuilts)
                        {
                          {"Array",typeof(Collections.List<>) },
                          {"Date", typeof(Date) },
                          {"Math", typeof(Math) }
                        };
                    }
                    else
                    {
                        importedTypes = new Dictionary<string, Type>();
                    }
                }
                return importedTypes;
            }
        }

        readonly ITypeContext parent;

        public TypeContext(ITypeContext parent)
        {
            this.parent = parent;
        }

        public Type GetType(TypeName typeName)
        {
            if (typeName.Namespace == null && TryGetType(typeName.Name, out Type type))
            {
                return type;
            }

            type = Type.GetType(typeName.FullName, false);
            if (type != null)
                return type;
            foreach (var item in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = item.GetType(typeName.FullName, false);
                if (type != null)
                    return type;
            }
            return null;
        }

        internal void Add(Type type)
        {
            ImportedTypes[type.Name] = type;
        }

        public void Register(string name, Type type)
        {
            ImportedTypes[name] = type;
        }

        public bool TryGetType(string name, out Type type)
        {
            if (ImportedTypes.TryGetValue(name, out type))
                return true;
            type = null;
            return parent != null && parent.TryGetType(name, out type);
        }
    }

    public class DefaultProgramContext : TypeProvider, ITypeContext
    {
        void ITypeContext.Register(string name, Type type)
        {
            throw new NotSupportedException(nameof(ITypeContext.Register));
        }

        public bool TryGetType(string name, out Type type)
        {
            return Inbuilts.TryGetValue(name, out type);
        }

        Type ITypeContext.GetType(TypeName typeName)
        {
            if (typeName.Namespace == null && Inbuilts.TryGetValue(typeName.Name, out Type t))
                return t;
            var type = Type.GetType(typeName.FullName, false);
            if (type != null)
                return type;
            foreach (var item in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = item.GetType(typeName.FullName, false);
                if (type != null)
                    return type;
            }
            return null;
        }
    }
}
