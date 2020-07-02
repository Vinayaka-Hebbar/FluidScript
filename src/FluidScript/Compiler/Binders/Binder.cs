﻿using FluidScript.Compiler.Emit;
using System;

namespace FluidScript.Compiler.Binders
{
    #region Binder
    /// <summary>
    /// Binder for variable or member
    /// </summary>
    public interface IBinder
    {
        void GenerateGet(MethodBodyGenerator generator);

        void GenerateSet(MethodBodyGenerator generator);

        Type Type { get; }

        /// <summary>
        /// From this you can identify whether Binder is a Variable or Static Member 
        /// </summary>
        bool CanEmitThis { get; }

        /// <summary>
        ///  From this you can identify whether Binder is a Variable or Member 
        /// </summary>
        bool IsMember { get; }

        object Get(object obj);

        void Set(object obj, object value);
    }

    public interface IBinderProvider
    {
        IBinder Binder { get; }
    }
    #endregion

    #region Dynamic Variable

    internal
#if LATEST_VS
        readonly
#endif
        struct EmptyBinder : IBinder
    {
        public EmptyBinder(Type type)
        {
            Type = type;
        }

        public Type Type { get; }

        public bool CanEmitThis => false;

        public bool IsMember => false;

        public void GenerateGet(MethodBodyGenerator generator)
        {

        }

        public void GenerateSet(MethodBodyGenerator generator)
        {

        }

        public object Get(object obj)
        {
            return null;
        }

        public void Set(object obj, object value)
        {

        }
    }

    public
#if LATEST_VS
        readonly
#endif
        struct DynamicVariableBinder : IBinder
    {
        readonly ILocalVariable variable;
        readonly System.Runtime.CompilerServices.IRuntimeVariables target;

        public DynamicVariableBinder(ILocalVariable variable, System.Runtime.CompilerServices.IRuntimeVariables target)
        {
            this.variable = variable;
            this.target = target;
        }

        public Type Type => variable.Type;

        public bool CanEmitThis => false;

        public bool IsMember => false;

        public void GenerateGet(MethodBodyGenerator generator)
        {
            generator.DeclareVariable(variable.Type, variable.Name);
        }

        public void GenerateSet(MethodBodyGenerator generator)
        {
            var iLVariable = generator.GetLocalVariable(variable.Name);
            if (iLVariable != null)
                generator.LoadVariable(iLVariable);
        }

        public object Get(object obj)
        {
            return target[variable.Index];
        }

        public void Set(object obj, object value)
        {
            target[variable.Index] = value;
        }
    }
    #endregion

    #region RuntimeVariable
    /// <summary>
    /// IL generator for <see cref="RuntimeVariables"/>
    /// </summary>
    public
#if LATEST_VS
        readonly
#endif
        struct RuntimeVariableBinder : IBinder
    {
        readonly ILocalVariable variable;
        readonly System.Runtime.CompilerServices.IRuntimeVariables variables;

        public RuntimeVariableBinder(ILocalVariable variable, System.Runtime.CompilerServices.IRuntimeVariables variables)
        {
            this.variable = variable;
            this.variables = variables;
        }

        public Type Type => variable.Type;

        public bool CanEmitThis => false;

        public bool IsMember => false;

        public void GenerateGet(MethodBodyGenerator generator)
        {
            throw new NotImplementedException();
        }

        public void GenerateSet(MethodBodyGenerator generator)
        {
            throw new NotImplementedException();
        }

        public object Get(object obj)
        {
            return variables[variable.Index];
        }

        public void Set(object obj, object value)
        {
            variables[variable.Index] = variable;
        }
    }
    #endregion

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

        public void GenerateGet(MethodBodyGenerator generator)
        {
            generator.LoadVariable(variable);
        }

        public void GenerateSet(MethodBodyGenerator generator)
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
