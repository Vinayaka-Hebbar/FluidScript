using FluidScript.Compiler.Emit;
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

        public void GenerateGet(MethodBodyGenerator generator, MethodCompileOption option)
        {
            generator.Call(Getter);
        }

        public void GenerateSet(MethodBodyGenerator generator, MethodCompileOption option)
        {
            generator.Call(Setter);
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
