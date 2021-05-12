using System;
using System.Globalization;
using System.Reflection;

namespace FluidScript.Compiler.Generators
{
    public class PropertyInstantiation : PropertyInfo
    {
        private readonly PropertyInfo info;

        internal MethodInfo Getter;

        internal MethodInfo Setter;

        public PropertyInstantiation(PropertyInfo info)
        {
            this.info = info;
        }

        public override Type PropertyType => info.PropertyType;

        public override PropertyAttributes Attributes => info.Attributes;

        public override bool CanRead => info.CanRead;

        public override bool CanWrite => info.CanWrite;

        public override string Name => info.Name;

        public override Type DeclaringType => info.DeclaringType;

        public override Type ReflectedType => info.ReflectedType;

        public override MethodInfo[] GetAccessors(bool nonPublic)
        {
            return new MethodInfo[] { Getter, Setter };
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return info.GetCustomAttributes(inherit);
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return info.GetCustomAttributes(attributeType, inherit);
        }

        public override MethodInfo GetGetMethod(bool nonPublic)
        {
            return Getter;
        }

        public override ParameterInfo[] GetIndexParameters()
        {
            return info.GetIndexParameters();
        }

        public override MethodInfo GetSetMethod(bool nonPublic)
        {
            return Setter;
        }

        public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            throw new NotImplementedException();
        }

        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
