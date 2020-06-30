using FluidScript.Compiler.Emit;
using FluidScript.Extensions;
using FluidScript.Utils;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace FluidScript.Compiler.Generators
{
    public sealed class TypeGenerator : System.Type, IMemberGenerator
    {
        private const BindingFlags PublicInstanceOrStatic = PublicInstance | BindingFlags.Static;
        private const BindingFlags PublicInstance = BindingFlags.Public | BindingFlags.Instance;
        private const MethodAttributes DefaultStaticCtor = MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.HideBySig;
        private const MethodAttributes DefaultCtor = MethodAttributes.Public | MethodAttributes.HideBySig;

        internal readonly IList<IMemberGenerator> Members = new List<IMemberGenerator>();
        private readonly System.Reflection.Emit.TypeBuilder _builder;
        IList<AttributeGenerator> _customAttributes;

        public override Module Module => _builder.Module;

        private readonly AssemblyGen assemblyGen;

        public override string Name { get; }

        public System.Type Type => _builder;

        public IProgramContext Context { get; }

        public override System.Type BaseType { get; }

        public ITextSource Source
        {
            get;
            set;
        }

        internal bool TryGetProperty(string name, out PropertyGenerator property)
        {
            var member = Members.FirstOrDefault(mem => mem.MemberType == MemberTypes.Property && string.Equals(mem.Name, name, System.StringComparison.OrdinalIgnoreCase));
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
        public MemberInfo MemberInfo => _builder;

        /// <inheritdoc/>
        public override Assembly Assembly => _builder.Assembly;

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
            Context = new ProgramContext(assemblyGen.Context);
        }

        public void Add(IMemberGenerator generator)
        {
            Members.Add(generator);
        }

        public void Generate()
        {
            CreateType();
        }

        public System.Type CreateType()
        {
            if (Members.Any(mem => mem.MemberType == MemberTypes.Constructor) == false)
            {
                //default ctor
                var ctor = new ConstructorGenerator(_builder.DefineConstructor(DefaultCtor, CallingConventions.Standard, new System.Type[0]), new Emit.ParameterInfo[0], new System.Type[0], this)
                {
                    SyntaxBody = SyntaxTree.Statement.Empty
                };
                ctor.Generate();
            }
            if (Members.Any(mem => mem.MemberType == MemberTypes.Field && mem.IsStatic))
            {
                //check for static ctor
                if (Members.Any(mem => mem.MemberType == MemberTypes.Constructor && mem.IsStatic) == false)
                {
                    var ctor = new ConstructorGenerator(_builder.DefineConstructor(DefaultStaticCtor, CallingConventions.Standard, new System.Type[0]), new Emit.ParameterInfo[0], new System.Type[0], this)
                    {
                        SyntaxBody = Compiler.SyntaxTree.Statement.Empty
                    };
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
#if NETFRAMEWORK
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

        public bool IsStatic => _builder.IsAbstract && _builder.IsSealed;

        internal bool CanImplementMethod(string name, System.Type[] types, out string newName)
        {
            var methods = BaseType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(m => m.IsDefined(typeof(Runtime.RegisterAttribute), false) && System.Attribute.GetCustomAttribute(m, typeof(Runtime.RegisterAttribute)).Match(name)).ToArray();
            var selected = DefaultBinder.SelectMethod(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, methods, types, null);
            bool hasExist = selected != null;
            newName = hasExist ? selected.Name : name;
            return hasExist;
        }

        internal bool CanImplementProperty(string name, System.Type returnType, System.Type[] parameterTypes, out string newName)
        {
            var property = BaseType.GetProperty(name, PublicInstance, null, returnType, parameterTypes, new ParameterModifier[0]);
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

        /// <inheritdoc/>
        protected override TypeAttributes GetAttributeFlagsImpl()
        {
            return _builder.Attributes;
        }

        /// <inheritdoc/>
        protected override ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, System.Type[] types, ParameterModifier[] modifiers)
        {
            return _builder.GetConstructor(bindingAttr, binder, callConvention, types, modifiers);
        }

        /// <inheritdoc/>
        public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr)
        {
            return GetMembers<ConstructorInfo>(MemberTypes.Constructor, bindingAttr).ToArray();
        }

        /// <inheritdoc/>
        public override System.Type GetElementType()
        {
            return _builder.GetElementType();
        }

        /// <inheritdoc/>
        public override EventInfo GetEvent(string name, BindingFlags bindingAttr)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc/>
        public override EventInfo[] GetEvents(BindingFlags bindingAttr)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc/>
        public override FieldInfo GetField(string name, BindingFlags bindingAttr)
        {
            return GetMembers<FieldInfo>(MemberTypes.Field, name, bindingAttr).FirstOrDefault();
        }

        /// <inheritdoc/>
        public override FieldInfo[] GetFields(BindingFlags bindingAttr)
        {
            return GetMembers<FieldInfo>(MemberTypes.Field, bindingAttr).ToArray();
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
        public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
        {
            return InternalGetMembers(bindingAttr).ToArray();
        }

        /// <inheritdoc/>
        protected override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, System.Type[] types, ParameterModifier[] modifiers)
        {
            return _builder.GetMethod(name, bindingAttr, binder, callConvention, types, modifiers);
        }

        /// <inheritdoc/>
        public override MethodInfo[] GetMethods(BindingFlags bindingAttr)
        {
            return GetMembers<MethodInfo>(MemberTypes.Method, bindingAttr).ToArray();
        }

        /// <inheritdoc/>
        public override MemberInfo[] GetMember(string name, BindingFlags bindingAttr)
        {
            var member = Members.Where(m => m.IsEquals(name, bindingAttr)).Select(mem => mem.MemberInfo);
            if (member.Any())
                return member.ToArray();
            return BaseType.GetMember(name, bindingAttr);
        }

        public override MemberInfo[] GetMember(string name, MemberTypes type, BindingFlags bindingAttr)
        {
            var member = Members.Where(m => m.MemberType == type && m.IsEquals(name, bindingAttr)).Select(mem => mem.MemberInfo);
            if (member.Any())
                return member.ToArray();
            return BaseType.GetMember(name, type, bindingAttr);
        }

        /// <inheritdoc/>
        public override System.Type GetNestedType(string name, BindingFlags bindingAttr)
        {
            return GetMembers<System.Type>(MemberTypes.TypeInfo, bindingAttr).FirstOrDefault();
        }

        /// <inheritdoc/>
        public override System.Type[] GetNestedTypes(BindingFlags bindingAttr)
        {
            return GetMembers<System.Type>(MemberTypes.TypeInfo, bindingAttr).ToArray();
        }

        /// <inheritdoc/>
        public override PropertyInfo[] GetProperties(BindingFlags bindingAttr)
        {
            return GetMembers<PropertyInfo>(MemberTypes.Property, bindingAttr).ToArray();
        }

        /// <inheritdoc/>
        protected override PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder, System.Type returnType, System.Type[] types, ParameterModifier[] modifiers)
        {
            return _builder.GetProperty(name, bindingAttr, binder, returnType, types, modifiers);
        }

        /// <inheritdoc/>
        protected override bool HasElementTypeImpl()
        {
            return _builder.HasElementType;
        }

        /// <inheritdoc/>
        public override object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters)
        {
            return _builder.InvokeMember(name, invokeAttr, binder, target, args);
        }

        private IEnumerable<MemberInfo> InternalGetMembers(BindingFlags flags)
        {
            foreach (var member in Members)
            {
                if (member.BindingFlagsMatch(flags))
                {
                    yield return member.MemberInfo;
                }
            }
            if ((flags & BindingFlags.DeclaredOnly) == 0)
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

        private IEnumerable<T> GetMembers<T>(MemberTypes memberType, BindingFlags flags)
            where T : MemberInfo
        {
            foreach (var member in Members)
            {
                if (member.MemberType == memberType && member.BindingFlagsMatch(flags))
                {
                    yield return (T)member.MemberInfo;
                }
            }
            if ((flags & BindingFlags.DeclaredOnly) == 0)
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

        private IEnumerable<T> GetMembers<T>(MemberTypes memberType, string name, BindingFlags flags)
            where T : MemberInfo
        {
            foreach (var member in Members)
            {
                if (member.MemberType == memberType && member.Name == name && member.BindingFlagsMatch(flags))
                {
                    yield return (T)member.MemberInfo;
                }
            }
            if ((flags & BindingFlags.DeclaredOnly) == 0)
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
            if (_customAttributes != null)
                return _customAttributes.Select(att => att.Instance).ToArray();
            return new object[0];
        }

        /// <inheritdoc/>
        public override object[] GetCustomAttributes(System.Type attributeType, bool inherit)
        {
            if (_customAttributes != null)
            {
                return _customAttributes.Where(att => att.Type == attributeType || (inherit && att.Type.IsAssignableFrom(attributeType))).
                    Select(att => att.Instance).ToArray();
            }
            return new object[0];
        }

        public void SetCustomAttribute(System.Type type, ConstructorInfo ctor, object[] parameters)
        {
            if (_customAttributes == null)
                _customAttributes = new List<AttributeGenerator>();
            _customAttributes.Add(new AttributeGenerator(type, ctor, parameters, null, null));
        }

        /// <inheritdoc/>
        public override bool IsDefined(System.Type attributeType, bool inherit)
        {
            return _customAttributes != null && _customAttributes.Any(attr => attr.Type == attributeType || (inherit && attr.Type.IsAssignableFrom(attributeType)));
        }
    }
}
