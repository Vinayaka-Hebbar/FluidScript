using FluidScript.Compiler.Emit;
using System;

namespace FluidScript.Compiler.Binders
{
    #region Parameter Binder
    public
#if LATEST_VS
        readonly
#endif
        struct ParameterBinder : IBinder
    {
        readonly ParameterInfo parameter;

        public ParameterBinder(ParameterInfo parameter)
        {
            this.parameter = parameter;
        }

        public Type Type => parameter.Type;

        public bool CanEmitThis => false;

        public bool IsMember => false;

        public void GenerateGet(MethodBodyGenerator generator)
        {
            if (generator.Method.IsStatic)
                generator.LoadArgument(parameter.Index);
            else
                generator.LoadArgument(parameter.Index + 1);
        }

        public void GenerateSet(MethodBodyGenerator generator)
        {
            if (generator.Method.IsStatic)
                generator.StoreArgument(parameter.Index);
            else
                generator.StoreArgument(parameter.Index + 1);
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
