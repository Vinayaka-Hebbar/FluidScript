using FluidScript.Compiler.Emit;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace FluidScript.Compiler.Generators
{
    public sealed class TypeGenerator : System.Type, ITypeProvider
    {
        private const System.Reflection.BindingFlags PublicInstanceOrStatic = PublicInstance | System.Reflection.BindingFlags.Static;
        private const System.Reflection.BindingFlags PublicInstance = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance;
        private const System.Reflection.MethodAttributes DefaultStaticCtor = System.Reflection.MethodAttributes.Private | System.Reflection.MethodAttributes.Static | System.Reflection.MethodAttributes.HideBySig;
        private const System.Reflection.MethodAttributes DefaultCtor = System.Reflection.MethodAttributes.Public | System.Reflection.MethodAttributes.HideBySig;

        internal readonly IList<IMemberGenerator> Members = new List<IMemberGenerator>();
        private readonly System.Reflection.Emit.TypeBuilder _builder;
        public System.Reflection.Emit.ModuleBuilder ModuleGen => _builder.Module as System.Reflection.Emit.ModuleBuilder;
        public override System.Reflection.Module Module => _builder.Module;

        private readonly AssemblyGen assemblyGen;

        public override string Name { get; }

        public System.Type Type => _builder;

        public override System.Type BaseType { get; }

        public IScriptSource Source
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

        /// <summary>
        /// MemberType of Generator
        /// </summary>
        public System.Reflection.MemberInfo MemberInfo => _builder;

        /// <inheritdoc/>
        public override System.Reflection.Assembly Assembly => _builder.Assembly;

        /// <inheritdoc/>
        public override string AssemblyQualifiedName => _builder.AssemblyQualifiedName;

        /// <inheritdoc/>
        public override string FullName => _builder.FullName;

        /// <inheritdoc/>
        public override System.Guid GUID => _builder.GUID;

        /// <inheritdoc/>
        public override string Namespace => _builder.Namespace;

        /// <inheritdoc/>
        public override System.Type UnderlyingSystemType => this;

        public TypeGenerator(System.Reflection.Emit.TypeBuilder builder, AssemblyGen assemblyGen)
        {
            this.assemblyGen = assemblyGen;
            Name = builder.Name;
            _builder = builder;
            BaseType = _builder.BaseType;
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
                var ctor = new ConstructorGenerator(_builder.DefineConstructor(DefaultCtor, System.Reflection.CallingConventions.Standard, new System.Type[0]), new ParameterInfo[0], new System.Type[0], this, Compiler.SyntaxTree.Statement.Empty);
                ctor.Generate();
            }
            if (Members.Any(mem => mem.MemberType == System.Reflection.MemberTypes.Field && mem.IsStatic))
            {
                //check for static ctor
                if (Members.Any(mem => mem.MemberType == System.Reflection.MemberTypes.Constructor && mem.IsStatic) == false)
                {
                    var ctor = new ConstructorGenerator(_builder.DefineConstructor(DefaultStaticCtor, System.Reflection.CallingConventions.Standard, new System.Type[0]), new ParameterInfo[0], new System.Type[0], this, Compiler.SyntaxTree.Statement.Empty);
                    ctor.Generate();
                }
            }
            foreach (var generator in Members)
            {
                generator.Generate();
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
            return assemblyGen.DefineDocument(Source.Path);
        }
#endif

        internal System.Reflection.Emit.TypeBuilder Builder
        {
            get
            {
                return _builder;
            }
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

        public IEnumerable<System.Reflection.MemberInfo> FindMember(string name, System.Reflection.BindingFlags flags)
        {
            bool HasMember(System.Reflection.MemberInfo m)
            {
                if (m.IsDefined(typeof(Runtime.RegisterAttribute), false))
                {
                    var data = (System.Attribute)m.GetCustomAttributes(typeof(Runtime.RegisterAttribute), false).FirstOrDefault();
                    if (data != null)
                        return data.Match(name);
                }
                return name.Equals(name);
            }
            var member = Members.Where(m => m.BindingFlagsMatch(flags)).Select(mem => mem.MemberInfo).Where(HasMember);
            if (member.Any())
                return member;
            return BaseType.GetMembers(flags).Where(HasMember);
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
                member.Generate();
            }
        }

        public System.Type GetType(TypeName typeName)
        {
            return assemblyGen.GetType(typeName.FullName);
        }

        /// <inheritdoc/>
        protected override System.Reflection.TypeAttributes GetAttributeFlagsImpl()
        {
            return _builder.Attributes;
        }

        /// <inheritdoc/>
        protected override System.Reflection.ConstructorInfo GetConstructorImpl(System.Reflection.BindingFlags bindingAttr, System.Reflection.Binder binder, System.Reflection.CallingConventions callConvention, System.Type[] types, System.Reflection.ParameterModifier[] modifiers)
        {
            return _builder.GetConstructor(bindingAttr, binder, callConvention, types, modifiers);
        }

        /// <inheritdoc/>
        public override System.Reflection.ConstructorInfo[] GetConstructors(System.Reflection.BindingFlags bindingAttr)
        {
            return GetMembers<System.Reflection.ConstructorInfo>(System.Reflection.MemberTypes.Constructor, bindingAttr).ToArray();
        }

        /// <inheritdoc/>
        public override System.Type GetElementType()
        {
            return _builder.GetElementType();
        }

        /// <inheritdoc/>
        public override System.Reflection.EventInfo GetEvent(string name, System.Reflection.BindingFlags bindingAttr)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc/>
        public override System.Reflection.EventInfo[] GetEvents(System.Reflection.BindingFlags bindingAttr)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc/>
        public override System.Reflection.FieldInfo GetField(string name, System.Reflection.BindingFlags bindingAttr)
        {
            return GetMembers<System.Reflection.FieldInfo>(System.Reflection.MemberTypes.Field, name, bindingAttr).FirstOrDefault();
        }

        /// <inheritdoc/>
        public override System.Reflection.FieldInfo[] GetFields(System.Reflection.BindingFlags bindingAttr)
        {
            return GetMembers<System.Reflection.FieldInfo>(System.Reflection.MemberTypes.Field, bindingAttr).ToArray();
        }

        /// <inheritdoc/>
        public override System.Type GetInterface(string name, bool ignoreCase)
        {
            return _builder.GetInterface(name, ignoreCase);
        }

        /// <inheritdoc/>
        public override System.Type[] GetInterfaces()
        {
            return _builder.GetInterfaces();
        }

        /// <inheritdoc/>
        public override System.Reflection.MemberInfo[] GetMembers(System.Reflection.BindingFlags bindingAttr)
        {
            return InternalGetMembers(bindingAttr).ToArray();
        }

        /// <inheritdoc/>
        protected override System.Reflection.MethodInfo GetMethodImpl(string name, System.Reflection.BindingFlags bindingAttr, System.Reflection.Binder binder, System.Reflection.CallingConventions callConvention, System.Type[] types, System.Reflection.ParameterModifier[] modifiers)
        {
            return _builder.GetMethod(name, bindingAttr, binder, callConvention, types, modifiers);
        }

        /// <inheritdoc/>
        public override System.Reflection.MethodInfo[] GetMethods(System.Reflection.BindingFlags bindingAttr)
        {
            return GetMembers<System.Reflection.MethodInfo>(System.Reflection.MemberTypes.Method, bindingAttr).ToArray();
        }

        /// <inheritdoc/>
        public override System.Reflection.MemberInfo[] GetMember(string name, System.Reflection.BindingFlags bindingAttr)
        {
            return FindMember(name, bindingAttr).ToArray();
        }

        /// <inheritdoc/>
        public override System.Type GetNestedType(string name, System.Reflection.BindingFlags bindingAttr)
        {
            return GetMembers<System.Type>(System.Reflection.MemberTypes.TypeInfo, bindingAttr).FirstOrDefault();
        }

        /// <inheritdoc/>
        public override System.Type[] GetNestedTypes(System.Reflection.BindingFlags bindingAttr)
        {
            return GetMembers<System.Type>(System.Reflection.MemberTypes.TypeInfo, bindingAttr).ToArray();
        }

        /// <inheritdoc/>
        public override System.Reflection.PropertyInfo[] GetProperties(System.Reflection.BindingFlags bindingAttr)
        {
            return GetMembers<System.Reflection.PropertyInfo>(System.Reflection.MemberTypes.Property, bindingAttr).ToArray();
        }

        /// <inheritdoc/>
        protected override System.Reflection.PropertyInfo GetPropertyImpl(string name, System.Reflection.BindingFlags bindingAttr, System.Reflection.Binder binder, System.Type returnType, System.Type[] types, System.Reflection.ParameterModifier[] modifiers)
        {
            return _builder.GetProperty(name, bindingAttr, binder, returnType, types, modifiers);
        }

        /// <inheritdoc/>
        protected override bool HasElementTypeImpl()
        {
            return _builder.HasElementType;
        }

        /// <inheritdoc/>
        public override object InvokeMember(string name, System.Reflection.BindingFlags invokeAttr, System.Reflection.Binder binder, object target, object[] args, System.Reflection.ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters)
        {
            return _builder.InvokeMember(name, invokeAttr, binder, target, args);
        }

        private IEnumerable<System.Reflection.MemberInfo> InternalGetMembers(System.Reflection.BindingFlags flags)
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

        /// <inheritdoc/>
        protected override bool IsArrayImpl()
        {
            return _builder.IsArray;
        }

        /// <inheritdoc/>
        protected override bool IsByRefImpl()
        {
            return _builder.IsByRef;
        }

        /// <inheritdoc/>
        protected override bool IsCOMObjectImpl()
        {
            return _builder.IsCOMObject;
        }

        /// <inheritdoc/>
        protected override bool IsPointerImpl()
        {
            return _builder.IsPointer;
        }

        /// <inheritdoc/>
        protected override bool IsPrimitiveImpl()
        {
            return _builder.IsPrimitive;
        }

        /// <inheritdoc/>
        public override object[] GetCustomAttributes(bool inherit)
        {
            return _builder.GetCustomAttributes(inherit);
        }

        /// <inheritdoc/>
        public override object[] GetCustomAttributes(System.Type attributeType, bool inherit)
        {
            return _builder.GetCustomAttributes(attributeType, inherit);
        }

        /// <inheritdoc/>
        public override bool IsDefined(System.Type attributeType, bool inherit)
        {
            return _builder.IsDefined(attributeType, inherit);
        }
    }
}
