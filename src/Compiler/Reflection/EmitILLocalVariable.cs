using System;
using System.Reflection.Emit;

namespace FluidScript.Compiler.Reflection
{
    public sealed class EmitILLocalVariable : Emit.ILLocalVariable
    {
        public readonly LocalBuilder LocalBuilder;

        public EmitILLocalVariable(LocalBuilder local, string name)
        {
            LocalBuilder = local??throw new ArgumentNullException(nameof(local));
            Name = name;
        }

        public override int Index => LocalBuilder.LocalIndex;

        public override Type Type => LocalBuilder.LocalType;

        public override string Name { get; }
    }
}
