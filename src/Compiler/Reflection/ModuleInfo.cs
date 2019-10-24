using System.Collections.Generic;

namespace FluidScript.Compiler.Reflection
{
    public class ModuleInfo
    {
        public readonly string Name;
        public readonly IDictionary<string, TypeInfo> Types;
        public readonly AssemblyInfo Assembly;

        public static readonly ModuleInfo SystemModule;

        static ModuleInfo()
        {
            SystemModule = new ModuleInfo(typeof(object).Module.Name, AssemblyInfo.MSCorLib);
        }

        internal ModuleInfo(string name, AssemblyInfo assembly)
        {
            Name = name;
            Types = new Dictionary<string, TypeInfo>();
            Assembly = assembly;
        }

        public TypeInfo DefineType(string name, ConstructorInfo[] constructors, TypeInfo declareType, System.Reflection.TypeAttributes attributes, TypeInfo baseType)
        {
            TypeInfo typeInfo = new TypeInfo(name, constructors, declareType, attributes, this, baseType);
            Types.Add(name, typeInfo);
            return typeInfo;
        }

        public TypeInfo DefineType(string name, ConstructorInfo[] constructors, TypeInfo declareType, System.Reflection.TypeAttributes attributes)
        {
            TypeInfo typeInfo = new TypeInfo(name, constructors, declareType, attributes, this, TypeInfo.Object);
            Types.Add(name, typeInfo);
            return typeInfo;
        }
    }
}
