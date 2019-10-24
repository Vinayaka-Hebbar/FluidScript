using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FluidScript.Compiler.Reflection
{
    public class TypeInfo : MemberInfo
    {
        public static readonly TypeInfo Object;
        public readonly string Name;
        public readonly ModuleInfo Module;
        private IDictionary<string, FieldInfo> fields;
        private IDictionary<string, PropertyInfo> properties;
        private IList<MethodInfo> methods;
        public readonly System.Reflection.TypeAttributes Attributes;
        public readonly TypeInfo BaseType;
        public IEnumerable<ConstructorInfo> Constructors;

        private System.Type runtimeType;

        public const System.Reflection.TypeAttributes PublicSealed = System.Reflection.TypeAttributes.Public | System.Reflection.TypeAttributes.Sealed;

        private IList<TypeInfo> nestedTypes;

        public IEnumerable<TypeInfo> NestedTypes
        {
            get
            {
                if (nestedTypes == null)
                    return Enumerable.Empty<TypeInfo>();
                return nestedTypes;
            }
        }

        public IEnumerable<FieldInfo> Fields
        {
            get
            {
                if (fields == null)
                    return Enumerable.Empty<FieldInfo>();
                return fields.Values;
            }
        }

        public IEnumerable<MethodInfo> Methods
        {
            get
            {
                if (methods == null)
                    return Enumerable.Empty<MethodInfo>();
                return methods;
            }
        }

        public IEnumerable<PropertyInfo> Properties
        {
            get
            {
                if (properties == null)
                    return Enumerable.Empty<PropertyInfo>();
                return properties.Values;
            }
        }

        public TypeInfo(string name, System.Reflection.TypeAttributes attributes, ModuleInfo module, TypeInfo baseType) : base()
        {
            Name = name;
            Attributes = attributes;
            Module = module;
            BaseType = baseType;
        }

        public TypeInfo(string name, IEnumerable<ConstructorInfo> constructors, TypeInfo declaredType, System.Reflection.TypeAttributes attributes, ModuleInfo module, TypeInfo baseType) : base(declaredType)
        {
            Name = name;
            Constructors = constructors;
            Attributes = attributes;
            Module = module;
            BaseType = baseType;
        }

        private TypeInfo(System.Type type) : base(null)
        {
            Name = type.Name;
            Attributes = type.Attributes;
            runtimeType = type;
        }

        static TypeInfo()
        {
            System.Type type = typeof(object);
            Object = new TypeInfo(type.Name, ConstructorInfo.Empty, null, type.Attributes, ModuleInfo.SystemModule, null)
            {
                runtimeType = type
            };
        }

        public FieldInfo DeclareField(string name, TypeInfo type, System.Reflection.FieldAttributes attributes)
        {
            if (fields == null)
                fields = new Dictionary<string, FieldInfo>();
            if (fields.ContainsKey(name))
                throw new System.InvalidOperationException(string.Format("field {0} already present", name));
            var field = new FieldInfo(name, type, this, attributes);
            fields.Add(name, field);
            return field;
        }

        public PropertyInfo DeclareProperty(string name, TypeInfo type, MethodInfo getter, MethodInfo setter, System.Reflection.PropertyAttributes attributes)
        {
            if (properties == null)
                properties = new Dictionary<string, PropertyInfo>();
            if (properties.ContainsKey(name))
                throw new System.InvalidOperationException(string.Format("property {0} already present", name));
            var property = new PropertyInfo(name, type, this, getter, setter, attributes);
            properties.Add(name, property);
            return property;
        }

        public MethodInfo DeclareMethod(string name, ParameterInfo[] paramters, TypeInfo returnType, System.Reflection.MethodAttributes attributes)
        {
            if (methods == null)
                methods = new List<MethodInfo>();
            var existingMethods = methods.Where(info => info.Name.Equals(name));
            if (existingMethods.Any())
            {
                foreach (var info in existingMethods)
                {
                    var parameters = info.Parameters;
                    if (parameters.Where((parameter, i) => parameter.Type.Equals(paramters[i].Type)).Any())
                        throw new System.InvalidOperationException(string.Format("method {0} already present", name));
                }
            }
            var method = new MethodInfo(name, returnType, paramters, this, attributes);
            methods.Add(method);
            return method;
        }



        internal void DeclareMethod(MethodInfo method)
        {
            if (methods == null)
                methods = new List<MethodInfo>();
            var existingMethods = methods.Where(info => info.Name.Equals(method.Name));
            if (existingMethods.Any())
            {
                foreach (var info in existingMethods)
                {
                    var parameters = info.Parameters;
                    if (parameters.Where((parameter, i) => parameter.Type.Equals(method.Parameters[i].Type)).Any())
                        throw new System.InvalidOperationException(string.Format("method {0} already present", method.Name));
                }
            }
            methods.Add(method);

        }

        public bool IsGenerated => runtimeType != null;

        public override System.Reflection.MemberTypes Types => MemberTypes.TypeInfo;

        public void DeclareNestedType(TypeInfo type)
        {
            if (nestedTypes == null)
                nestedTypes = new List<TypeInfo>();
            if (nestedTypes.Any(ntype => ntype.Equals(type)))
                throw new System.InvalidOperationException(string.Format("type {0} already present", type));
            nestedTypes.Add(type);
        }

        public System.Type RuntimeType()
        {
            return runtimeType;
        }

        public void Generate(ModuleBuilder builder)
        {
            if (BaseType != null && BaseType.IsGenerated == false)
                BaseType.Generate(builder);
            var typeBuilder = builder.DefineType(this);
            foreach (var constructor in Constructors)
            {
                constructor.Generate(typeBuilder);
            }
            if (fields != null)
            {
                foreach (var field in fields.Values)
                {
                    field.Generate(typeBuilder);
                }
            }
            if (properties != null)
            {
                foreach (PropertyInfo property in properties.Values)
                {
                    property.Generate(typeBuilder);
                }
            }
            if (methods != null)
            {
                foreach (var method in methods)
                {
                    method.Generate(typeBuilder);
                }
            }
            runtimeType = typeBuilder.CreateType();
        }


        public static TypeInfo From(System.Type type)
        {
            return new TypeInfo(type);
        }
    }
}
