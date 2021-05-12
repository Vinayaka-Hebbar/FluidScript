using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Reflection.Emit;

namespace FluidScript.Compiler.Generators
{
    public class TypeBuilderInstantiation : Type, Emit.IRuntimeType
    {
        private readonly Type type;
        private readonly Type[] insts;
        private readonly Type instantiateType;

        public TypeBuilderInstantiation(Type type, params Type[] insts)
        {
            this.type = type;
            this.insts = insts;
            instantiateType = type.MakeGenericType(insts);
        }

        public override RuntimeTypeHandle TypeHandle => type.TypeHandle;

        public override Guid GUID => type.GUID;

        public override Module Module => type.Module;

        public override Assembly Assembly => type.Assembly;

        public override string FullName => type.FullName;

        public override string Namespace => type.Namespace;

        public override string AssemblyQualifiedName => type.AssemblyQualifiedName;

        public override Type BaseType => type.BaseType;

        public override Type UnderlyingSystemType => instantiateType;

        public override string Name => type.Name;

        public override bool ContainsGenericParameters => true;

        public override Type[] GetGenericArguments()
        {
            return insts;
        }

        public override Type GetGenericTypeDefinition()
        {
            return type;
        }

        public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr)
        {
            return type.GetConstructors(bindingAttr);
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return type.GetCustomAttributes(inherit);
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return type.GetCustomAttributes(attributeType, inherit);
        }

        public override Type GetElementType()
        {
            return type.GetElementType();
        }

        public override EventInfo GetEvent(string name, BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        public override EventInfo[] GetEvents(BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        public override FieldInfo GetField(string name, BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        public override FieldInfo[] GetFields(BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        public override Type GetInterface(string name, bool ignoreCase)
        {
            return type.GetInterface(name, ignoreCase);
        }

        public override Type[] GetInterfaces()
        {
            return type.GetInterfaces();
        }

        public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
        {
            return new MemberInfo[0];
        }

        public override MemberInfo[] GetMember(string name, MemberTypes type, BindingFlags bindingAttr)
        {
            if(type == MemberTypes.Property)
            return new PropertyInfo[0];
            if (type == MemberTypes.Method)
                return new MethodInfo[0];
            if (type == MemberTypes.Field)
                return new FieldInfo[0];
            return new MemberInfo[0];
        }

        public override MethodInfo[] GetMethods(BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        public override Type GetNestedType(string name, BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        public override Type[] GetNestedTypes(BindingFlags bindingAttr)
        {
            return type.GetNestedTypes(bindingAttr);
        }

        public override PropertyInfo[] GetProperties(BindingFlags bindingAttr)
        {
            return new PropertyInfo[0];
        }

        public override object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters)
        {
            throw new NotImplementedException();
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return type.IsDefined(attributeType, inherit);
        }

        public override MemberInfo[] GetDefaultMembers()
        {
            MemberInfo[] members = type.GetDefaultMembers();
            if (members.Length > 0)
            {
                int length = members.Length;
                List<MemberInfo> res = new List<MemberInfo>(length);
                for (int i = 0; i < length; i++)
                {
                    MemberInfo member = members[i];
                    if (member.MemberType == MemberTypes.Property)
                    {
                        var property = (PropertyInfo)member;
                        MethodInfo getMethod = property.GetMethod;
                        var propertyInstant = new PropertyInstantiation(property);
                        if (getMethod.ContainsGenericParameters)
                            propertyInstant.Getter = TypeBuilder.GetMethod(instantiateType, getMethod);
                        MethodInfo setMethod = property.SetMethod;
                        if (setMethod.ContainsGenericParameters)
                            propertyInstant.Setter = TypeBuilder.GetMethod(instantiateType, setMethod);
                        res.Add(propertyInstant);
                    }
                    else
                    {
                        res.Add(member);
                    }
                }
                return res.ToArray();

            }
            return members;
        }

        protected override TypeAttributes GetAttributeFlagsImpl()
        {
            return type.Attributes;
        }

        protected override ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
        {
            return type.GetConstructor(bindingAttr, binder, callConvention, types, modifiers);
        }

        protected override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
        {
            throw new NotImplementedException();
        }

        protected override PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
        {
            throw new NotImplementedException();
        }

        public override bool IsAssignableFrom(Type c)
        {
            if (c is TypeBuilderInstantiation inst)
            {
                if (type != inst.type)
                    return false;
                if (insts.Length != inst.insts.Length)
                    return false;
                for (int i = 0; i < insts.Length; i++)
                {
                    if (inst.insts[i] != insts[i])
                        return false;
                }
                return true;
            }
            return base.IsAssignableFrom(c);
        }

        protected override bool HasElementTypeImpl()
        {
            return type.HasElementType;
        }

        protected override bool IsArrayImpl()
        {
            return type.IsArray;
        }

        protected override bool IsByRefImpl()
        {
            return type.IsByRef;
        }

        protected override bool IsCOMObjectImpl()
        {
            return type.IsCOMObject;
        }

        protected override bool IsPointerImpl()
        {
            return type.IsPointer;
        }

        protected override bool IsPrimitiveImpl()
        {
            return type.IsPrimitive;
        }


    }
}
