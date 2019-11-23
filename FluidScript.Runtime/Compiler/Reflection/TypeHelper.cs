using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FluidScript.Compiler.Reflection
{
#if Runtime
    public static class TypeHelper
    {
        private const BindingFlags DeclaredOnly = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
        private const BindingFlags Any = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;

        public static RuntimeObject Invoke(object instance, string name, RuntimeObject[] args)
        {
            var type = instance.GetType();
            var types = args.Select(arg => arg.DeclaredType).ToArray();
            var methods = type.GetMethods(Any)
                .Where(m => m.IsDefined(typeof(Callable), false));
            var method = methods.FirstOrDefault(m =>
            {
                var attribute = m.GetCustomAttributes(typeof(Callable), false).First();
                return ((Callable)attribute).Name == name;
            });
            if (method == null)
                return RuntimeObject.Undefined;
            var value = (RuntimeObject)method.Invoke(instance, args);
            return value;
        }

        public static RuntimeObject Invoke(object instance, string name)
        {
            var type = instance.GetType();
            var methods = type.GetMembers(DeclaredOnly)
                .Where(m => m.IsDefined(typeof(Callable), false));
            var member = methods.FirstOrDefault(m =>
            {
                var attribute = m.GetCustomAttributes(typeof(Callable), false).First();
                return ((Callable)attribute).Name == name;
            });
            if (member == null)
                return RuntimeObject.Undefined;
            switch (member.MemberType)
            {
                case MemberTypes.Property:
                    var property = (PropertyInfo)member;
                    return (RuntimeObject)property.GetValue(instance, new object[0]);
                case MemberTypes.Method:
                    var method = (MethodInfo)member;
                    return (RuntimeObject)method.Invoke(instance, new object[0]);
            }
            return RuntimeObject.Null;
        }

        public static IEnumerable<DeclaredMethod> GetMethods(Type type)
        {
            var methods = type.GetMethods(DeclaredOnly)
                 .Where(m => m.IsDefined(typeof(Callable), false));
            foreach (MethodInfo method in methods)
            {
                yield return GetMethod(method);
            }
        }

        public static DeclaredMethod GetMethod(MethodInfo method)
        {
            var attribute = (Callable)method.GetCustomAttributes(typeof(Callable), false).First();
            var parameters = method.GetParameters();
            var argTypes = attribute.Arguments;
            var arguments = new Emit.ArgumentType[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                var arg = argTypes[i];
                var flags = ArgumentFlags.None;
                if ((arg & Emit.ArgumentTypes.VarArg) == Emit.ArgumentTypes.VarArg)
                {
                    arg ^= Emit.ArgumentTypes.VarArg;
                    flags |= ArgumentFlags.VarArgs;
                }
                arguments[i] = new Emit.ArgumentType(parameters[i].Name, (RuntimeType)arg, flags);
            }
            var declaredMethod = new DeclaredMethod(attribute.Name, arguments, attribute.ReturnType) { Store = method, Attributes = method.Attributes };
            if (method.IsStatic)
            {
                declaredMethod.Default = new Metadata.FunctionReference(null, arguments, attribute.ReturnType, method);
            }
            return declaredMethod;
        }

        public static IEnumerable<DeclaredField> GetFields(Type type)
        {
            var fields = type.GetFields(DeclaredOnly)
                .Where(f => f.IsDefined(typeof(Field), false));
            foreach (var field in fields)
            {
                yield return GetField(field);
            }
        }

        public static DeclaredField GetField(FieldInfo field)
        {
            var attribute = (Field)field.GetCustomAttributes(typeof(Field), false).First();
            var declaredField = new DeclaredField(attribute.Name, attribute.Type) { Attributes = field.Attributes };
            if (field.IsStatic)
            {
                declaredField.DefaultValue = (RuntimeObject)field.GetValue(null);
            }

            return declaredField;
        }

        public static DeclaredProperty GetProperty(PropertyInfo property)
        {
            DeclaredMethod getter = null, setter = null;
            var attribute = (Property)property.GetCustomAttributes(typeof(Property), false).First();
            var getMethod = property.GetGetMethod(true);
            if (getMethod != null)
            {
                getter = new DeclaredMethod(attribute.Name, new Emit.ArgumentType[0], attribute.Type) { Store = getMethod, Attributes = getMethod.Attributes };
                if (getMethod.IsStatic)
                {
                    getter.Default = new Metadata.FunctionReference(null, getter.Arguments, attribute.Type, getMethod);
                }
            }
            var setMethod = property.GetSetMethod(true);
            if (setMethod != null)
            {
                setter = new DeclaredMethod(attribute.Name, new Emit.ArgumentType[1] { new Emit.ArgumentType("value", attribute.Type) }, RuntimeType.Void) { Store = setMethod, Attributes = setMethod.Attributes };
                if (setMethod.IsStatic)
                {
                    setter.Default = new Metadata.FunctionReference(null, setter.Arguments, RuntimeType.Void, setMethod);
                }
            }
            return new DeclaredProperty(attribute.Name, attribute.Type, getter, setter) { Attributes = property.Attributes };
        }

        public static IEnumerable<DeclaredMember> GetMembers(Type type)
        {
            var members = type.GetMembers(DeclaredOnly).Where(m => m.IsDefined(typeof(Accessable), true));
            foreach (var member in members)
            {
                switch (member.MemberType)
                {
                    case MemberTypes.Field:
                        yield return GetField((FieldInfo)member);
                        break;
                    case MemberTypes.Method:
                        yield return GetMethod((MethodInfo)member);
                        break;
                    case MemberTypes.Property:
                        yield return GetProperty((PropertyInfo)member);
                        break;
                }
            }
        }
    }
#endif
}
