using FluidScript.Compiler.Emit;
using FluidScript.Compiler.SyntaxTree;
using System;

namespace FluidScript.Compiler.Binders
{
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

        public BindingAttributes Attributes
        {
            get
            {
                if (Getter != null)
                    return BindingAttributes.Member | (Getter.IsStatic ? BindingAttributes.None : BindingAttributes.HasThis);
                if (Setter != null)
                    return BindingAttributes.Member | (Setter.IsStatic ? BindingAttributes.None : BindingAttributes.HasThis);
                return BindingAttributes.Member;
                
            }
        }

        public void GenerateGet(Expression target, MethodBodyGenerator generator, MethodCompileOption option)
        {
            if ((option & MethodCompileOption.EmitStartAddress) == MethodCompileOption.EmitStartAddress && property.PropertyType.IsValueType)
            {
                var temp = generator.CreateTemporaryVariable(Type);
                generator.Call(Getter);
                generator.StoreVariable(temp);
                generator.LoadAddressOfVariable(temp);
                return;
            }
            generator.Call(Getter);
        }

        public void GenerateSet(Expression value, MethodBodyGenerator generator, MethodCompileOption option)
        {
            if ((option & MethodCompileOption.Dupplicate) == 0)
            {
                generator.Call(Setter);
                return;
            }
            generator.Duplicate();
            var temp = generator.CreateTemporaryVariable(value.Type);
            generator.StoreVariable(temp);
            generator.Call(Setter);
            generator.LoadVariable(temp);
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
