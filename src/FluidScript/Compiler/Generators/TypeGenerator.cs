using FluidScript.Compiler.Emit;
using FluidScript.Compiler.SyntaxTree;
using FluidScript.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace FluidScript.Compiler.Generators
{
    public sealed class TypeGenerator : Type, IType
    {
        const MethodAttributes DefaultStaticCtor = MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.HideBySig;
        const MethodAttributes DefaultCtor = MethodAttributes.Public | MethodAttributes.HideBySig;
        const TypeAttributes NestedTypeAttr = TypeAttributes.NestedPrivate | TypeAttributes.Class;

        internal readonly IList<IMember> Members = new List<IMember>();
        private readonly TypeBuilder _builder;
        IList<AttributeGenerator> _customAttributes;
        Type[] interfaces;

        int nestedTypes;

        public override Module Module => _builder.Module;

        private readonly AssemblyGen assemblyGen;

        public override string Name { get; }

        public override Type ReflectedType => _builder;

        public ITypeContext Context { get; }

        public override Type BaseType { get; }

        public ITextSource Source
        {
            get;
            set;
        }

        internal bool TryGetProperty(string name, out PropertyGenerator property)
        {
            var member = Members.FirstOrDefault(mem => mem.MemberType == MemberTypes.Property && string.Equals(mem.Name, name, StringComparison.OrdinalIgnoreCase));
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
        public override Guid GUID => _builder.GUID;

        /// <inheritdoc/>
        public override string Namespace => _builder.Namespace;

        /// <inheritdoc/>
        public override Type UnderlyingSystemType => _builder;

        public TypeGenerator(TypeBuilder builder, AssemblyGen assemblyGen)
        {
            this.assemblyGen = assemblyGen;
            Name = builder.Name;
            _builder = builder;
            BaseType = _builder.BaseType;
            Context = new TypeContext(assemblyGen.Context);
        }

        public TypeGenerator(TypeBuilder builder, AssemblyGen assemblyGen, ITypeContext context)
        {
            this.assemblyGen = assemblyGen;
            Name = builder.Name;
            _builder = builder;
            BaseType = _builder.BaseType;
            Context = context;
        }

        public void SetInterfaces(Type[] interfaces)
        {
            for (int i = 0; i < interfaces.Length; i++)
            {
                _builder.AddInterfaceImplementation(interfaces[i]);
            }
            this.interfaces = interfaces;
        }

        public void Add(IMember generator)
        {
            Members.Add(generator);
        }

        public void Compile()
        {
            CreateType();
        }

        public Type CreateType()
        {
            if (_customAttributes != null)
            {
                foreach (var attr in _customAttributes)
                {
                    var cuAttr = new CustomAttributeBuilder(attr.Ctor, attr.Parameters);
                    _builder.SetCustomAttribute(cuAttr);
                }
            }
            if (Members.Any(mem => mem.MemberType == MemberTypes.Constructor) == false)
            {
                //default ctor
                IMember ctor = new ConstructorGenerator(_builder.DefineConstructor(DefaultCtor, CallingConventions.Standard, new Type[0]), new Emit.ParameterInfo[0], this)
                {
                    SyntaxBody = Statement.Empty
                };
                ctor.Compile();
            }
            if (Members.Any(mem => mem.MemberType == MemberTypes.Field && mem.IsStatic))
            {
                //check for static ctor
                if (Members.Any(mem => mem.MemberType == MemberTypes.Constructor && mem.IsStatic) == false)
                {
                    IMember ctor = new ConstructorGenerator(_builder.DefineConstructor(DefaultStaticCtor, CallingConventions.Standard, new Type[0]), new Emit.ParameterInfo[0], this)
                    {
                        SyntaxBody = Statement.Empty
                    };
                    ctor.Compile();
                }
            }
            foreach (var generator in Members)
            {
                generator.Compile();
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

        internal TypeBuilder Builder
        {
            get
            {
                return _builder;
            }
        }

        public bool IsStatic => _builder.IsAbstract && _builder.IsSealed;

        /// <summary>
        /// Checks whether a method can be implemented or not
        /// </summary>
        /// <param name="name">Origincal name</param>
        /// <param name="types">Types</param>
        /// <param name="newName">New name of implemented method</param>
        /// <param name="returnType">Return type for the implemented method</param>
        /// <param name="attrs">Method attributes</param>
        /// <returns></returns>
        internal void CheckImplementMethod(string name, Type[] types, ref string newName, ref Type returnType, ref MethodAttributes attrs)
        {
            var selected = BaseType.FindMethod(name, types, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            if (selected != null)
            {
                // if can be overiden
                if (selected.IsVirtual && selected.IsAssembly == false)
                {
                    // is not hidden method
                    if ((selected.Attributes & MethodAttributes.NewSlot) == MethodAttributes.NewSlot
                            && (selected.Attributes & MethodAttributes.Final) == MethodAttributes.Final
                    && (selected.Attributes & MethodAttributes.Private) == MethodAttributes.Private)
                    {
                        attrs &= ~MethodAttributes.Public;
                        attrs |= MethodAttributes.Final | MethodAttributes.Private | MethodAttributes.NewSlot | MethodAttributes.Virtual;
                    }
                    else if ((selected.Attributes & MethodAttributes.Final) == 0)
                    {
                        attrs |= MethodAttributes.ReuseSlot;
                    }
                    newName = selected.Name;
                    returnType = selected.ReturnType;
                    return;
                }
                throw new InvalidOperationException($"Method {name} does not support impl");
            }
            else
            {
                attrs |= MethodAttributes.NewSlot;
            }
        }

        public void Build()
        {
            foreach (var member in Members)
            {
                member.Compile();
            }
        }

        /// <inheritdoc/>
        protected override TypeAttributes GetAttributeFlagsImpl()
        {
            return _builder.Attributes;
        }

        /// <inheritdoc/>
        protected override ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
        {
            return _builder.GetConstructor(bindingAttr, binder, callConvention, types, modifiers);
        }

        /// <inheritdoc/>
        public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr)
        {
            return GetMembers<ConstructorInfo>(MemberTypes.Constructor, bindingAttr).ToArray();
        }

        /// <inheritdoc/>
        public override Type GetElementType()
        {
            return _builder.GetElementType();
        }

        /// <inheritdoc/>
        public override EventInfo GetEvent(string name, BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override EventInfo[] GetEvents(BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
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
        public override Type GetInterface(string name, bool ignoreCase)
        {
            if (interfaces == null)
                return null;
            var comparision = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.CurrentCulture;
            return interfaces.FirstOrDefault(t => t.FullName.Equals(name, comparision));
        }

        /// <inheritdoc/>
        public override Type[] GetInterfaces()
        {
            return _builder.GetInterfaces();
        }

        /// <inheritdoc/>
        public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
        {
            return InternalGetMembers(bindingAttr).ToArray();
        }

        /// <inheritdoc/>
        protected override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
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
            {
                switch (type)
                {
                    case MemberTypes.Constructor:
                        return member.Cast<ConstructorInfo>().ToArray();
                    case MemberTypes.Event:
                        return member.Cast<EventInfo>().ToArray();
                    case MemberTypes.Field:
                        return member.Cast<FieldInfo>().ToArray();
                    case MemberTypes.Method:
                        return member.Cast<MethodInfo>().ToArray();
                    case MemberTypes.Property:
                        return member.Cast<PropertyInfo>().ToArray();
                    case MemberTypes.TypeInfo:
                        return member.Cast<Type>().ToArray();
                    default:
                        return member.ToArray();
                }
            }

            return BaseType.GetMember(name, type, bindingAttr);
        }

        /// <inheritdoc/>
        public override Type GetNestedType(string name, BindingFlags bindingAttr)
        {
            return GetMembers<Type>(MemberTypes.TypeInfo, bindingAttr).FirstOrDefault();
        }

        /// <inheritdoc/>
        public override Type[] GetNestedTypes(BindingFlags bindingAttr)
        {
            return GetMembers<Type>(MemberTypes.TypeInfo, bindingAttr).ToArray();
        }

        /// <inheritdoc/>
        public override PropertyInfo[] GetProperties(BindingFlags bindingAttr)
        {
            return GetMembers<PropertyInfo>(MemberTypes.Property, bindingAttr).ToArray();
        }

        /// <inheritdoc/>
        protected override PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
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
                for (Type type = BaseType; type != null; type = type.BaseType)
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
                for (Type type = BaseType; type != null; type = type.BaseType)
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
                for (Type type = this.BaseType; type != null; type = type.BaseType)
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
        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            if (_customAttributes != null)
            {
                return _customAttributes.Where(att => att.Type == attributeType || (inherit && att.Type.IsAssignableFrom(attributeType))).
                    Select(att => att.Instance).ToArray();
            }
            return new object[0];
        }

        public void SetCustomAttribute(Type type, ConstructorInfo ctor, object[] parameters)
        {
            if (_customAttributes == null)
                _customAttributes = new List<AttributeGenerator>();
            _customAttributes.Add(new AttributeGenerator(type, ctor, parameters, null, null));
        }

        /// <inheritdoc/>
        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return _customAttributes != null && _customAttributes.Any(attr => attr.Type == attributeType || (inherit && attr.Type.IsAssignableFrom(attributeType)));
        }

        public MethodGenerator DefineMethod(string name, MethodAttributes attrs, Emit.ParameterInfo[] parameters, Type returnType)
        {
            var method = _builder.DefineMethod(name, attrs, CallingConventions.Standard, returnType, parameters.Map(p => p.Type));
            MethodGenerator methodGen = new MethodGenerator(method, parameters, this);
            Members.Add(methodGen);
            return methodGen;
        }

        public ConstructorGenerator DefineCtor(Emit.ParameterInfo[] parameters, MethodAttributes attrs)
        {
            var ctor = _builder.DefineConstructor(attrs, CallingConventions.Standard, parameters.Map(p => p.Type));
            ConstructorGenerator ctorGen = new ConstructorGenerator(ctor, parameters, this);
            Members.Add(ctorGen);
            return ctorGen;
        }

        public FieldGenerator DefineField(string name, Type type, FieldAttributes attrs)
        {
            FieldGenerator fieldGen = new FieldGenerator(this, attrs, new VariableDeclarationExpression(name, TypeSyntax.Create(type), null));
            Members.Add(fieldGen);
            return fieldGen;
        }

        public TypeGenerator DefineNestedType(string name, Type parent, TypeAttributes attr)
        {
            TypeBuilder builder = this._builder.DefineNestedType(string.Concat(Namespace, ".", name), attr | NestedTypeAttr, parent);
            var generator = new TypeGenerator(builder, assemblyGen, Context);
            Context.Register(name, generator);
            return generator;
        }

        public LamdaGen DefineAnonymousMethod(Type[] types, Type returnType)
        {
            // todo: Anonomous type name
            var builder = _builder.DefineNestedType("DisplayClass_" + nestedTypes, LamdaGen.Attributes | TypeAttributes.NestedPrivate, typeof(object));
            nestedTypes++;
            var values = builder.DefineField("Values", LamdaGen.ObjectArray, FieldAttributes.Private);
            var ctor = builder.DefineConstructor(DelegateGen.CtorAttributes, CallingConventions.Standard, LamdaGen.CtorSignature);
            var method = builder.DefineMethod("Invoke", MethodAttributes.HideBySig, CallingConventions.Standard, returnType, types);
            var iLGen = ctor.GetILGenerator();
            iLGen.Emit(OpCodes.Ldarg_0);
            iLGen.Emit(OpCodes.Call, typeof(object).GetConstructor(EmptyTypes));
            iLGen.Emit(OpCodes.Ldarg_0);
            iLGen.Emit(OpCodes.Ldarg_1);
            iLGen.Emit(OpCodes.Stfld, values);
            iLGen.Emit(OpCodes.Ret);

            // Values = values;
            return new LamdaGen(builder, method)
            {
                Constructor = ctor,
                Values = values
            };
        }
    }
}
