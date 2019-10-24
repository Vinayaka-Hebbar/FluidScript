namespace FluidScript.Compiler.Reflection
{
    public class ModuleBuilder
    {
        public readonly ModuleInfo Module;
        public readonly AssemblyBuilder Builder;
        public readonly System.Reflection.Emit.ModuleBuilder DynamicModuleBuilder;

        internal ModuleBuilder(ModuleInfo module, AssemblyBuilder builder, System.Reflection.Emit.ModuleBuilder dynamicModuleBuilder)
        {
            Module = module;
            Builder = builder;
            DynamicModuleBuilder = dynamicModuleBuilder;
        }

        public System.Reflection.Emit.TypeBuilder DefineType(TypeInfo type)
        {
            return DynamicModuleBuilder.DefineType(type.Name, type.Attributes);
        }
    }
}
