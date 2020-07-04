using FluidScript.Compiler.Emit;
using System;

namespace FluidScript.Compiler.Binders
{
    #region Variable Binder
    public
#if LATEST_VS
        readonly
#endif
        struct VariableBinder : IBinder
    {
        readonly ILLocalVariable variable;

        public VariableBinder(ILLocalVariable variable)
        {
            this.variable = variable;
        }

        public Type Type => variable.Type;

        public bool CanEmitThis => false;

        public bool IsMember => false;

        public void GenerateGet(MethodBodyGenerator generator, MethodCompileOption option)
        {
            generator.LoadVariable(variable);
        }

        public void GenerateSet(MethodBodyGenerator generator, MethodCompileOption option)
        {
            generator.StoreVariable(variable);
        }

        public object Get(object obj)
        {
            throw new NotSupportedException(nameof(Get));
        }

        public void Set(object obj, object value)
        {
            throw new NotSupportedException(nameof(Set));
        }
    }
    #endregion
}
