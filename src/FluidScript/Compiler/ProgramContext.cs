using FluidScript.Compiler.Emit;
using System;
using System.Collections.Generic;

namespace FluidScript.Compiler
{
    public class ProgramContext : IProgramContext
    {
        static DefaultProgramContext defaultContext;
        public static DefaultProgramContext Default
        {
            get
            {
                if (defaultContext == null)
                    defaultContext = new DefaultProgramContext();
                return defaultContext;
            }
        }

        public bool IsRuntimeSupported => false;

        Dictionary<string, Type> importedTypes;
        internal IDictionary<string, Type> ImportedTypes
        {
            get
            {
                if (importedTypes == null)
                    importedTypes = new Dictionary<string, Type>();
                return importedTypes;
            }
        }

        readonly IProgramContext parent;

        public ProgramContext(IProgramContext parent)
        {
            this.parent = parent;
        }

        public Type GetType(TypeName typeName)
        {
            if (typeName.Namespace == null &&
                (TypeProvider.Inbuilts.TryGetValue(typeName.Name, out Type type) ||
                TryGetType(typeName.Name, out type)))
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
            type = null;
            return (importedTypes != null && importedTypes.TryGetValue(name, out type)) || 
                (parent != null && parent.TryGetType(name, out type));
        }
    }

    public class DefaultProgramContext : TypeProvider, IProgramContext
    {
        public bool IsRuntimeSupported => false;

        void IProgramContext.Register(string name, Type type)
        {
            throw new NotSupportedException(nameof(IProgramContext.Register));
        }

        public bool TryGetType(string name, out Type type)
        {
            return Inbuilts.TryGetValue(name, out type);
        }
    }
}
