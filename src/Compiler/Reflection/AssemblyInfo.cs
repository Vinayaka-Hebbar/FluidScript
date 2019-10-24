using System.Collections.Generic;
using System.Linq;

namespace FluidScript.Compiler.Reflection
{
    public class AssemblyInfo
    {
        public static readonly AssemblyInfo MSCorLib;

        static AssemblyInfo()
        {
            MSCorLib = new AssemblyInfo(typeof(object).Assembly.FullName);
        }

        public IList<ModuleInfo> Modules;
        public readonly string Name;

        public AssemblyInfo(string name)
        {
            Name = name;
        }

        public ModuleInfo NewModule(string name)
        {
            if (Modules == null)
                Modules = new List<ModuleInfo>();
            if (Modules.Any(module => module.Name.Equals(name)))
                throw new System.InvalidOperationException(string.Format("module {0} already present", name));
            ModuleInfo moduleInfo = new ModuleInfo(name, this);
            Modules.Add(moduleInfo);
            return moduleInfo;
        }
    }
}
