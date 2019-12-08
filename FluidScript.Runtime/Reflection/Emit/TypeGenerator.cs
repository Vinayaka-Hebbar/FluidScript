using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace FluidScript.Reflection.Emit
{
    public sealed class TypeGenerator : System.Type, ITypeProvider
    {
        private const System.Reflection.BindingFlags PublicInstanceOrStatic = PublicInstance | System.Reflection.BindingFlags.Static;
        private const System.Reflection.BindingFlags PublicInstance = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance;
        internal readonly IList<IMemberGenerator> Members = new List<IMemberGenerator>();
        private readonly System.Reflection.Emit.TypeBuilder _builder;
        public readonly ReflectionModule ReflectionModule;
        public override System.Reflection.Module Module { get; }

        public override string Name { get; }

        public System.Type Type => _builder;

        public override System.Type BaseType { get; }

        public Library.IScriptSource Source
        {
            get;
            set;
        }

        internal bool TryGetProperty(string name, out PropertyGenerator property)
        {
            var member = Members.FirstOrDefault(mem => mem.MemberType == System.Reflection.MemberTypes.Property && string.Equals(mem.Name, name, System.StringComparison.OrdinalIgnoreCase));
            if (member != null)
            {
                property = (PropertyGenerator)member;
                return true;
            }
            property = null;
            return false;
        }

        public System.Reflection.MemberInfo MemberInfo => _builder;

        public override System.Reflection.Assembly Assembly => _builder.Assembly;

        public override string AssemblyQualifiedName => _builder.AssemblyQualifiedName;

        public override string FullName => _builder.FullName;

        public override System.Guid GUID => _builder.GUID;

        public override string Namespace => _builder.Namespace;

        public override System.Type UnderlyingSystemType => this;

        public TypeGenerator(System.Reflection.Emit.TypeBuilder builder, ReflectionModule module)
        {
            Name = builder.Name;
            _builder = builder;
            BaseType = _builder.BaseType;
            ReflectionModule = module;
            Module = builder.Module;
        }

        internal TypeGenerator()
        {
            Module = typeof(object).Module;
        }

        public void Add(IMemberGenerator generator)
        {
            Members.Add(generator);
        }

        public System.Type Create()
        {
            if (Members.Any(mem => mem.MemberType == System.Reflection.MemberTypes.Constructor) == false)
            {
                //default ctor
                var ctor = new ConstructorGenerator(_builder.DefineConstructor(System.Reflection.MethodAttributes.Public, System.Reflection.CallingConventions.Standard, new System.Type[0]), new System.Type[0], new System.Type[0], this, Compiler.SyntaxTree.Statement.Empty);
                ctor.Build();
            }
            if (Members.Any(mem => mem.MemberType == System.Reflection.MemberTypes.Field && mem.IsStatic))
            {
                //check for static ctor
                if (Members.Any(mem => mem.MemberType == System.Reflection.MemberTypes.Constructor && mem.IsStatic) == false)
                {
                    var ctor = new ConstructorGenerator(_builder.DefineConstructor(System.Reflection.MethodAttributes.Public | System.Reflection.MethodAttributes.Static, System.Reflection.CallingConventions.Standard, new System.Type[0]), new System.Type[0], new System.Type[0], this, Compiler.SyntaxTree.Statement.Empty);
                    ctor.Build();
                }
            }
            foreach (var generator in Members)
            {
                generator.Build();
            }
#if NETSTANDARD
            return _builder.CreateTypeInfo();
#else
            return _builder.CreateType();
#endif
        }
#if NET40
        public System.Diagnostics.SymbolStore.ISymbolDocumentWriter CreateDocumentWriter()
        {
            return ReflectionModule.DefineDocument(Source.Path);
        }
#endif

        internal System.Reflection.Emit.TypeBuilder GetBuilder()
        {
            return _builder;
        }

        public IEnumerable<System.Reflection.MemberInfo> FindMember(string name)
        {
            bool HasMember(System.Reflection.MemberInfo m)
            {
                if (m.IsDefined(typeof(Runtime.RegisterAttribute), false))
                {
                    var data = (System.Attribute)m.GetCustomAttributes(typeof(Runtime.RegisterAttribute), false).FirstOrDefault();
                    if (data != null)
                        return data.Match(name);
                }
                return m.Name == name;
            }
            var member = Members.Select(mem => mem.MemberInfo).Where(HasMember);
            if (member.Any())
                return member;
            return BaseType.GetMembers(PublicInstanceOrStatic).Where(HasMember);
        }

        internal bool CanImplementMethod(string name, System.Type[] types, out string newName)
        {
            var methods = BaseType.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Where(m => m.IsDefined(typeof(Runtime.RegisterAttribute), false) && System.Attribute.GetCustomAttribute(m, typeof(Runtime.RegisterAttribute)).Match(name)).ToArray();
            var selected = DefaultBinder.SelectMethod(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, methods, types, null);
            bool hasExist = selected != null;
            newName = hasExist ? selected.Name : name;
            return hasExist;
        }

        internal bool CanImplementProperty(string name, System.Type returnType, System.Type[] parameterTypes, out string newName)
        {
            var property = BaseType.GetProperty(name, PublicInstance, null, returnType, parameterTypes, new System.Reflection.ParameterModifier[0]);
            bool hasExist = property != null;
            newName = hasExist ? property.Name : name;
            return hasExist;
        }

        public void Build()
        {
            foreach (var member in Members)
            {
                member.Build();
            }
        }

        public new System.Type GetType(string typeName)
        {
            if (TypeUtils.IsInbuiltType(typeName))
                return TypeUtils.GetInbuiltType(typeName);
            return Module.GetType(typeName);
        }

        protected override System.Reflection.TypeAttributes GetAttributeFlagsImpl()
        {
            return _builder.Attributes;
        }

        protected override System.Reflection.ConstructorInfo GetConstructorImpl(System.Reflection.BindingFlags bindingAttr, System.Reflection.Binder binder, System.Reflection.CallingConventions callConvention, System.Type[] types, System.Reflection.ParameterModifier[] modifiers)
        {
            return _builder.GetConstructor(bindingAttr, binder, callConvention, types, modifiers);
        }

        public override System.Reflection.ConstructorInfo[] GetConstructors(System.Reflection.BindingFlags bindingAttr)
        {
            return GetMembers<System.Reflection.ConstructorInfo>(System.Reflection.MemberTypes.Constructor, bindingAttr).ToArray();
        }

        public override System.Type GetElementType()
        {
            return _builder.GetElementType();
        }

        public override System.Reflection.EventInfo GetEvent(string name, System.Reflection.BindingFlags bindingAttr)
        {
            throw new System.NotImplementedException();
        }

        public override System.Reflection.EventInfo[] GetEvents(System.Reflection.BindingFlags bindingAttr)
        {
            throw new System.NotImplementedException();
        }

        public override System.Reflection.FieldInfo GetField(string name, System.Reflection.BindingFlags bindingAttr)
        {
            return GetMembers<System.Reflection.FieldInfo>(System.Reflection.MemberTypes.Field, name, bindingAttr).FirstOrDefault();
        }

        public override System.Reflection.FieldInfo[] GetFields(System.Reflection.BindingFlags bindingAttr)
        {
            return GetMembers<System.Reflection.FieldInfo>(System.Reflection.MemberTypes.Field, bindingAttr).ToArray();
        }

        public override System.Type GetInterface(string name, bool ignoreCase)
        {
            return _builder.GetInterface(name, ignoreCase);
        }

        public override System.Type[] GetInterfaces()
        {
            return _builder.GetInterfaces();
        }

        public override System.Reflection.MemberInfo[] GetMembers(System.Reflection.BindingFlags bindingAttr)
        {
            return __GetMembers(bindingAttr).ToArray();
        }

        protected override System.Reflection.MethodInfo GetMethodImpl(string name, System.Reflection.BindingFlags bindingAttr, System.Reflection.Binder binder, System.Reflection.CallingConventions callConvention, System.Type[] types, System.Reflection.ParameterModifier[] modifiers)
        {
            return _builder.GetMethod(name, bindingAttr, binder, callConvention, types, modifiers);
        }

        public override System.Reflection.MethodInfo[] GetMethods(System.Reflection.BindingFlags bindingAttr)
        {
            return GetMembers<System.Reflection.MethodInfo>(System.Reflection.MemberTypes.Method, bindingAttr).ToArray();
        }

        public override System.Type GetNestedType(string name, System.Reflection.BindingFlags bindingAttr)
        {
            return GetMembers<System.Type>(System.Reflection.MemberTypes.TypeInfo, bindingAttr).FirstOrDefault();
        }

        public override System.Type[] GetNestedTypes(System.Reflection.BindingFlags bindingAttr)
        {
            return GetMembers<System.Type>(System.Reflection.MemberTypes.TypeInfo, bindingAttr).ToArray();
        }

        public override System.Reflection.PropertyInfo[] GetProperties(System.Reflection.BindingFlags bindingAttr)
        {
            return GetMembers<System.Reflection.PropertyInfo>(System.Reflection.MemberTypes.Property, bindingAttr).ToArray();
        }

        protected override System.Reflection.PropertyInfo GetPropertyImpl(string name, System.Reflection.BindingFlags bindingAttr, System.Reflection.Binder binder, System.Type returnType, System.Type[] types, System.Reflection.ParameterModifier[] modifiers)
        {
            return _builder.GetProperty(name, bindingAttr, binder, returnType, types, modifiers);
        }

        protected override bool HasElementTypeImpl()
        {
            return _builder.HasElementType;
        }

        public override object InvokeMember(string name, System.Reflection.BindingFlags invokeAttr, System.Reflection.Binder binder, object target, object[] args, System.Reflection.ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters)
        {
            return _builder.InvokeMember(name, invokeAttr, binder, target, args);
        }

        private IEnumerable<System.Reflection.MemberInfo> __GetMembers(System.Reflection.BindingFlags flags)
        {
            foreach (var member in Members)
            {
                if (member.BindingFlagsMatch(flags))
                {
                    yield return member.MemberInfo;
                }
            }
            if ((flags & System.Reflection.BindingFlags.DeclaredOnly) == 0)
            {
                for (System.Type type = BaseType; type != null; type = type.BaseType)
                {
                    foreach (var member in type.GetMembers(flags))
                    {
                        yield return member;
                    }
                }
            }
        }

        private IEnumerable<T> GetMembers<T>(System.Reflection.MemberTypes memberType, System.Reflection.BindingFlags flags)
            where T : System.Reflection.MemberInfo
        {
            foreach (var member in Members)
            {
                if (member.MemberType == memberType && member.BindingFlagsMatch(flags))
                {
                    yield return (T)member.MemberInfo;
                }
            }
            if ((flags & System.Reflection.BindingFlags.DeclaredOnly) == 0)
            {
                for (System.Type type = this.BaseType; type != null; type = type.BaseType)
                {
                    foreach (var member in type.GetMembers(flags))
                    {
                        if (member.MemberType == memberType)
                        {
                            yield return (T)member;
                        }
                    }
                }
            }
        }

        private IEnumerable<T> GetMembers<T>(System.Reflection.MemberTypes memberType, string name, System.Reflection.BindingFlags flags)
            where T : System.Reflection.MemberInfo
        {
            foreach (var member in Members)
            {
                if (member.MemberType == memberType && member.Name == name && member.BindingFlagsMatch(flags))
                {
                    yield return (T)member.MemberInfo;
                }
            }
            if ((flags & System.Reflection.BindingFlags.DeclaredOnly) == 0)
            {
                for (System.Type type = this.BaseType; type != null; type = type.BaseType)
                {
                    foreach (var member in type.GetMembers(flags))
                    {
                        if (member.MemberType == memberType && member.Name == name)
                        {
                            yield return (T)member;
                        }
                    }
                }
            }
        }

        protected override bool IsArrayImpl()
        {
            return _builder.IsArray;
        }

        protected override bool IsByRefImpl()
        {
            return _builder.IsByRef;
        }

        protected override bool IsCOMObjectImpl()
        {
            return _builder.IsCOMObject;
        }

        protected override bool IsPointerImpl()
        {
            return _builder.IsPointer;
        }

        protected override bool IsPrimitiveImpl()
        {
            return _builder.IsPrimitive;
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return _builder.GetCustomAttributes(inherit);
        }

        public override object[] GetCustomAttributes(System.Type attributeType, bool inherit)
        {
            return _builder.GetCustomAttributes(attributeType, inherit);
        }

        public override bool IsDefined(System.Type attributeType, bool inherit)
        {
            return _builder.IsDefined(attributeType, inherit);
        }
    }
}
