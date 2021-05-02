using System;
using System.Reflection;

namespace FluidScript.Runtime
{
    public interface IMemberBinder
    {
        Type Type { get; }
        object Get(object obj);
        void Set(object obj, object value);
    }

    internal
#if LATEST_VS
        readonly
#endif
        struct PropertyBinder : IMemberBinder
    {
        private readonly PropertyInfo property;

        public PropertyBinder(PropertyInfo property)
        {
            this.property = property;
        }

        public Type Type => property.PropertyType;

        public object Get(object obj)
        {
            if (!property.CanRead)
                throw new System.MemberAccessException(string.Concat("Cannot read value from readonly property ", property.Name));
            return property.GetValue(obj, new object[0]);
        }

        public void Set(object obj, object value)
        {
            if (!property.CanWrite)
                throw new System.MemberAccessException(string.Concat("Cannot write to readonly property ", property.Name));
            property.SetValue(obj, value, new object[0]);
        }
    }

    internal
#if LATEST_VS
        readonly
#endif
        struct FieldBinder : IMemberBinder
    {
        private readonly FieldInfo field;

        public FieldBinder(FieldInfo field)
        {
            this.field = field;
        }

        public Type Type => field.FieldType;

        public object Get(object obj)
        {
            return field.GetValue(obj);
        }

        public void Set(object obj, object value)
        {
            field.SetValue(obj, value);
        }
    }

    internal
#if LATEST_VS
        readonly
#endif
        struct DynamicBinder : IMemberBinder
    {
        private readonly MemberKey member;

        public DynamicBinder(MemberKey member)
        {
            this.member = member;
        }

        public Type Type => member.Type;

        public object Get(object obj)
        {
            return ((IDynamicObject)obj).GetValue(member);
        }

        public void Set(object obj, object value)
        {
            ((IDynamicObject)obj).SetValue(member, value);
        }
    }
}
