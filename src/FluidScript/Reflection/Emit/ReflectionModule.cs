using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Reflection.Emit;

namespace FluidScript.Reflection.Emit
{
    public sealed class ReflectionModule
    {
        public readonly AssemblyBuilder Assembly;
        public readonly ModuleBuilder Module;
        public int TypeCount;
        private readonly System.Collections.Generic.IDictionary<TypeName, System.Type> _types;

        public string Name { get; }

        /// <summary>
        /// Gets the language type GUID for the symbol store.
        /// </summary>
        private static readonly System.Guid LanguageType =      // FluidScript
            new System.Guid("1720F685-2505-4E68-9A76-65C156EE41C0");

        /// <summary>
        /// Gets the language vendor GUID for the symbol store.
        /// </summary>
        private static readonly System.Guid LanguageVendor =
            new System.Guid("F35723DB-4115-4B09-80EF-AFAE70FAD5F2");


        /// <summary>
        /// Gets the document type GUID for the symbol store.
        /// </summary>
        private static readonly System.Guid DocumentType =
            new System.Guid("A8103375-5141-463F-8472-7079C1A64314");

        public ReflectionModule(AssemblyBuilder assembly, ModuleBuilder module)
        {
            Assembly = assembly;
            Module = module;
            Name = module.ScopeName;
            _types = TypeUtils.Inbuilts.ToDictionary(item => (TypeName)item.Name, item => item.Type);
        }

        public TypeGenerator DefineType(string name, System.Reflection.TypeAttributes attr, System.Type parent)
        {
            var generator = new TypeGenerator(Module.DefineType(string.Concat(Name, ".", name), attr, parent), this);
            _types.Add(name, generator);
            return generator;
        }

        public System.Type GetType(TypeName typeName)
        {
            if (_types.TryGetValue(typeName, out System.Type type))
            {
                return type;
            }
            return Module.GetType(typeName.ToString());
        }

#if NET40
        internal ISymbolDocumentWriter DefineDocument(string url)
        {
            return Module.DefineDocument(url, LanguageType, LanguageVendor, DocumentType);
        }
#endif
    }
}
