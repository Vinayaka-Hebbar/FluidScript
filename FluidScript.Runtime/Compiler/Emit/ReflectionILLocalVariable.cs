using System;

namespace FluidScript.Compiler.Emit
{
    public class ReflectionILLocalVariable : ILLocalVariable
    {
        public System.Reflection.Emit.LocalBuilder UnderlyingLocal;

        public ReflectionILLocalVariable(System.Reflection.Emit.LocalBuilder local, string name)
        {
            UnderlyingLocal = local;
            Name = name;
        }

        public override int Index => UnderlyingLocal.LocalIndex;

        public override Type Type => UnderlyingLocal.LocalType;

        public override string Name { get; }
    }
}
