#if NETFRAMEWORK || MONOANDROID
using System.Diagnostics.SymbolStore;
#endif
using FluidScript.Compiler.Generators;
using System.Reflection.Emit;

namespace FluidScript.Compiler.Emit
{
    public sealed class AssemblyGen 
    {
        public readonly AssemblyBuilder Assembly;
        public readonly ModuleBuilder Module;
        public int TypeCount;

        private int dynamicCount;

        public string Namespace { get; }
        public ProgramContext Context { get; }

#if NETFRAMEWORK || MONOANDROID

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

#endif
        private static AssemblyGen _dynamicGen;
        public static AssemblyGen DynamicAssembly
        {
            get
            {
                if (_dynamicGen == null)
                {
                    System.Threading.Interlocked.CompareExchange(ref _dynamicGen, new AssemblyGen("Dynamic.Snippets", "1.0.0.0"), null);
                }
                return _dynamicGen;
            }
        }

        public AssemblyGen(string assemblyName, string version)
        {
            System.Reflection.AssemblyName name = new System.Reflection.AssemblyName(string.Concat(assemblyName, ", Version=", version));
#if NETFRAMEWORK || MONOANDROID
            var assembly = System.Threading.Thread.GetDomain().DefineDynamicAssembly(name, AssemblyBuilderAccess.RunAndSave);
            var module = assembly.DefineDynamicModule(assemblyName, string.Concat(assemblyName, ".dll"), false);
#else
            var assembly =
          AssemblyBuilder.DefineDynamicAssembly(name, AssemblyBuilderAccess.RunAndCollect);
            var module =
               assembly.DefineDynamicModule(name.Name);
#endif
            Assembly = assembly;
            Module = module;
            Namespace = module.ScopeName;
            Context = new ProgramContext(null);
        }

        public TypeGenerator DefineType(string name, System.Type parent, System.Reflection.TypeAttributes attr)
        {
            TypeBuilder builder = Module.DefineType(string.Concat(Namespace, ".", name), attr, parent);
            var generator = new TypeGenerator(builder, this);
            Context.Register(name, generator);
            return generator;
        }

        public TypeBuilder DefineDynamicType(string name, System.Type parent, System.Reflection.TypeAttributes attr)
        {
            int index = System.Threading.Interlocked.Increment(ref dynamicCount);
            return Module.DefineType(string.Concat(name, "$", index), attr, parent);
        }

        public System.Type GetType(string typeName)
        {
            if (Context.TryGetType(typeName, out System.Type type))
            {
                return type;
            }
            return Module.GetType(typeName);
        }

#if NETFRAMEWORK || MONOANDROID
        internal ISymbolDocumentWriter DefineDocument(string url)
        {
            return Module.DefineDocument(url, LanguageType, LanguageVendor, DocumentType);
        }

        public void Save(string path)
        {
            Assembly.Save(path, System.Reflection.PortableExecutableKinds.ILOnly, System.Reflection.ImageFileMachine.I386);
        }
#endif
    }
}
