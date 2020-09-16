using FluidScript.Compiler.Emit;
using FluidScript.Compiler.SyntaxTree;
using FluidScript.Runtime;
using System;

namespace FluidScript.Compiler.Binders
{
    #region Binder

    /// <summary>
    /// Binder for variable or member
    /// </summary>
    public interface IBinder
    {
        void GenerateGet(Expression target, MethodBodyGenerator generator, MethodCompileOption option = 0);

        void GenerateSet(Expression value, MethodBodyGenerator generator, MethodCompileOption option = 0);

        /// <summary>
        /// From this you can identify whether Binder is a Variable or Static Member 
        /// </summary>
        BindingAttributes Attributes { get; }

        Type Type { get; }

        object Get(object obj);

        void Set(object obj, object value);
    }

    public enum BindingAttributes
    {
        None,
        HasThis = 1,
        Member = 2,
        Dynamic = 4,
    }

    public interface IBindable
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

        public BindingAttributes Attributes => BindingAttributes.None;

        public void GenerateGet(Expression target, MethodBodyGenerator generator, MethodCompileOption option)
        {

        }

        public void GenerateSet(Expression value, MethodBodyGenerator generator, MethodCompileOption option)
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
        readonly MemberKey key;
        readonly System.Runtime.CompilerServices.IRuntimeVariables target;

        public DynamicVariableBinder(MemberKey key, System.Runtime.CompilerServices.IRuntimeVariables target)
        {
            this.key = key;
            this.target = target;
        }

        public Type Type => key.Type;

        public bool CanEmitThis => false;

        public BindingAttributes Attributes => BindingAttributes.None;

        public void GenerateGet(Expression target, MethodBodyGenerator generator, MethodCompileOption option)
        {
            generator.DeclareVariable(key.Type, key.Name);
        }

        public void GenerateSet(Expression value, MethodBodyGenerator generator, MethodCompileOption option)
        {
            var iLVariable = generator.GetLocalVariable(key.Name);
            if (iLVariable != null)
                generator.LoadVariable(iLVariable);
        }

        public object Get(object obj)
        {
            return target[key.Index];
        }

        public void Set(object obj, object value)
        {
            Type dest = key.Type;
            if (value == null)
            {
                if (dest.IsNullAssignable())
                    target[key.Index] = value;
                else
                    throw new Exception(string.Concat("Can't assign null value to type ", dest));
                return;
            }
            Type type = value.GetType();
            if (TypeUtils.AreReferenceAssignable(dest, type))
            {
                target[key.Index] = value;
            }
            else if (type.TryImplicitConvert(dest, out System.Reflection.MethodInfo implConvert))
            {
                value = implConvert.Invoke(null, new object[1] { value });
                target[key.Index] = value;
            }
            else
            {
                throw new System.InvalidCastException(string.Concat(type, " to ", dest));
            }
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

        public BindingAttributes Attributes => BindingAttributes.None;

        public void GenerateGet(Expression target, MethodBodyGenerator generator, MethodCompileOption option)
        {
            throw new NotImplementedException();
        }

        public void GenerateSet(Expression value, MethodBodyGenerator generator, MethodCompileOption option)
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
}
