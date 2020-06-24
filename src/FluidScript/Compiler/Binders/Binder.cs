using FluidScript.Compiler.Emit;
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

    #region Field Binder
    public
#if LATEST_VS
        readonly
#endif
        struct FieldBinder : IBinder
    {
        readonly System.Reflection.FieldInfo field;

        public FieldBinder(System.Reflection.FieldInfo field)
        {
            this.field = field;
        }

        public Type Type => field.FieldType;

        public bool CanEmitThis => field.IsStatic == false;

        public bool IsMember => true;

        public void GenerateGet(MethodBodyGenerator generator)
        {
            var field = this.field;
            if (field.FieldType == null)
                throw new InvalidOperationException(string.Concat("Use of undeclared field ", field));
            if (field is Generators.FieldGenerator)
                field = ((Generators.FieldGenerator)field).FieldInfo;
            generator.LoadField(field);
        }

        public void GenerateSet(MethodBodyGenerator generator)
        {
            var field = this.field;
            if (field.IsInitOnly && !(generator.Method is Generators.ConstructorGenerator))
                throw new FieldAccessException("A readonly field cannot be assigned to (except in a constructor of the class in which the field is defined or a variable initializer))");
                if (field is Generators.FieldGenerator)
                field = ((Generators.FieldGenerator)field).FieldInfo;
            generator.StoreField(field);
        }

        public object Get(object obj)
        {
            return field.GetValue(obj);
        }

        public void Set(object obj, object value)
        {
            field.SetValue(obj, value);
        }
    }
    #endregion

    #region Property Binder
    public struct PropertyBinder : IBinder
    {
        readonly System.Reflection.PropertyInfo property;

        public PropertyBinder(System.Reflection.PropertyInfo property) : this()
        {
            this.property = property;
        }

        private System.Reflection.MethodInfo m_getter;
        public System.Reflection.MethodInfo Getter
        {
            get
            {
                if (m_getter == null)
                    m_getter = property.GetGetMethod(true);
                return m_getter;
            }
        }

        private System.Reflection.MethodInfo m_setter;
        public System.Reflection.MethodInfo Setter
        {
            get
            {
                if (m_setter == null)
                    m_setter = property.GetSetMethod(true);
                return m_setter;
            }
        }

        public Type Type => property.PropertyType;

        public bool CanEmitThis
        {
            get
            {
                if (Getter != null)
                    return Getter.IsStatic == false;
                if (Setter != null)
                    return Setter.IsStatic == false;
                return false;
            }
        }

        public bool IsMember => true;

        public void GenerateGet(MethodBodyGenerator generator)
        {
            var get = Getter;
            if (get is IMethodBaseGenerator)
                get = (System.Reflection.MethodInfo)((IMethodBaseGenerator)get).MethodBase;
            generator.Call(get);
        }

        public void GenerateSet(MethodBodyGenerator generator)
        {
            var set = Setter;
            if (set is IMethodBaseGenerator)
                set = (System.Reflection.MethodInfo)((IMethodBaseGenerator)set).MethodBase;
            generator.Call(set);
        }

        public object Get(object obj)
        {
            var p = property;
            if (!p.CanRead)
                throw new MemberAccessException(string.Concat("Cannot read value from readonly property ", p.Name));
            return p.GetValue(obj, new object[0]);
        }

        public void Set(object obj, object value)
        {
            var p = property;
            if (!p.CanWrite)
                throw new MemberAccessException(string.Concat("Cannot write to readonly property ", p.Name));
            p.SetValue(obj, value, new object[0]);
        }
    }
    #endregion

}
