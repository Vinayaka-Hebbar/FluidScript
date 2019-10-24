namespace FluidScript.Compiler.Reflection
{
    public class AssemblyBuilder
    {
        public readonly AssemblyInfo Assembly;
        public System.Reflection.Emit.AssemblyBuilder DynamicAssemblyBuilder;
        public readonly System.Reflection.Emit.AssemblyBuilderAccess Access;

        public AssemblyBuilder(AssemblyInfo assembly, System.Reflection.Emit.AssemblyBuilderAccess access)
        {
            Assembly = assembly;
            Access = access;
        }

        public ModuleBuilder DefineModule(ModuleInfo module, bool emitDebugInfo)
        {
            if (DynamicAssemblyBuilder == null)
                DynamicAssemblyBuilder = System.Threading.Thread.GetDomain().DefineDynamicAssembly(new System.Reflection.AssemblyName(Assembly.Name), Access);
            var moduleBuilder = DynamicAssemblyBuilder.DefineDynamicModule(module.Name, emitDebugInfo);
            return new ModuleBuilder(module, this, moduleBuilder);
        }
    }
}
