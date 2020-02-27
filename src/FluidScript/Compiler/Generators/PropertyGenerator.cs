using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace FluidScript.Compiler.Generators
{
    public sealed class PropertyGenerator : System.Reflection.PropertyInfo, Emit.IMemberGenerator
    {
        public sealed class PropertyHolder
        {
            public readonly PropertyType PropertyType;
            public readonly MethodGenerator Method;

            public readonly bool IsPublic;
            public readonly bool IsPrivate;
            public readonly bool IsStatic;

            public PropertyHolder(PropertyType propertyType, MethodGenerator generator)
            {
                PropertyType = propertyType;
                Method = generator;
                IsPublic = generator.IsPublic;
                IsPrivate = generator.IsPrivate;
                IsStatic = generator.IsStatic;
            }
        }

        private IList<AttributeGenerator> _CustomAttributes;

        internal readonly TypeGenerator TypeGenerator;

        private readonly System.Reflection.Emit.PropertyBuilder _builder;

        public PropertyGenerator(TypeGenerator generator, System.Reflection.Emit.PropertyBuilder builder)
        {
            _builder = builder;
            TypeGenerator = generator;
            Name = builder.Name;
            MemberType = System.Reflection.MemberTypes.Property;
        }

        public override string Name { get; }

        public System.Reflection.MemberInfo MemberInfo => this;

        public override System.Reflection.MemberTypes MemberType { get; }

        public bool IsStatic
        {
            get
            {
                var first = Accessors.FirstOrDefault();
                if (first == null)
                    throw new Exception("Can't decide wether property is static or not");
                return first.IsStatic;
            }
        }

        public bool IsPublic
        {
            get
            {
                var first = Accessors.FirstOrDefault();
                if (first == null)
                    throw new Exception("Can't decide wether property is static or not");
                return first.IsPublic;
            }
        }

        public IList<PropertyHolder> Accessors { get; } = new List<PropertyHolder>(2);

        public override Type PropertyType => _builder.PropertyType;

        public override System.Reflection.PropertyAttributes Attributes => _builder.Attributes;

        public override bool CanRead => _builder.CanRead;

        public override bool CanWrite => _builder.CanWrite;

        public override Type DeclaringType => TypeGenerator;

        public override Type ReflectedType => TypeGenerator;

        public bool BindingFlagsMatch(System.Reflection.BindingFlags flags)
        {
            return Utils.TypeUtils.BindingFlagsMatch(IsPublic, flags, System.Reflection.BindingFlags.Public, System.Reflection.BindingFlags.NonPublic)
                           && Utils.TypeUtils.BindingFlagsMatch(IsStatic, flags, System.Reflection.BindingFlags.Static, System.Reflection.BindingFlags.Instance);
        }

        public override void SetValue(object obj, object value, System.Reflection.BindingFlags invokeAttr, System.Reflection.Binder binder, object[] index, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override System.Reflection.MethodInfo[] GetAccessors(bool nonPublic)
        {
            return Accessors.Where(acc => acc.IsPublic == nonPublic).Select(acc => acc.Method).ToArray();
        }

        public override System.Reflection.MethodInfo GetGetMethod(bool nonPublic)
        {
            PropertyHolder item = Accessors.FirstOrDefault(acc => acc.PropertyType == Generators.PropertyType.Get && acc.IsPublic == nonPublic);
            if (item != null)
                return item.Method;
            throw new Exception("Item Not Found");
        }

        public override System.Reflection.MethodInfo GetSetMethod(bool nonPublic)
        {
            return Accessors.FirstOrDefault(acc => acc.PropertyType == Generators.PropertyType.Set && acc.IsPublic == nonPublic).Method;
        }

        public override System.Reflection.ParameterInfo[] GetIndexParameters()
        {
            //todo indexer types
            return new System.Reflection.ParameterInfo[0];
        }

        public override object GetValue(object obj, System.Reflection.BindingFlags invokeAttr, System.Reflection.Binder binder, object[] index, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public void SetCustomAttribute(Type type, System.Reflection.ConstructorInfo ctor, object[] parameters)
        {
            if (_CustomAttributes == null)
                _CustomAttributes = new List<AttributeGenerator>();
            _CustomAttributes.Add(new AttributeGenerator(type, ctor, parameters, null, null));
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            if (_CustomAttributes != null)
                return _CustomAttributes.Select(att => att.Instance).ToArray();
            return new object[0];
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            if (_CustomAttributes != null)
            {
                var enumerable = _CustomAttributes.Where(att => att.Type == attributeType);
                return enumerable.Select(att => att.Instance).ToArray();
            }
            return new object[0];
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return _CustomAttributes != null && _CustomAttributes.Any(attr => attr.Type == attributeType || (inherit && attr.Type.IsAssignableFrom(attributeType)));
        }

        internal System.Reflection.Emit.PropertyBuilder GetBuilder()
        {
            return _builder;
        }


        public void Generate()
        {
            if (_CustomAttributes != null)
            {
                foreach (var attr in _CustomAttributes)
                {
                    var cuAttr = new System.Reflection.Emit.CustomAttributeBuilder(attr.Ctor, attr.Parameters);
                    _builder.SetCustomAttribute(cuAttr);
                }
            }
            foreach (var accessor in Accessors)
            {
                accessor.Method.Generate();
            }
        }

        public override string ToString()
        {
            return Name;
        }

        public void Initialize()
        {
            
        }
    }

    public enum PropertyType { Get, Set, Indexer }
}
