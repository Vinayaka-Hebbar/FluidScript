using FluidScript.Compiler;
using FluidScript.Compiler.Binders;
using FluidScript.Runtime;
using FluidScript.Utils;
using System.Reflection;

namespace FluidScript.Extensions
{
    public static class TypeExtensions
    {
        private const string Separator = ", ";
        private const BindingFlags DeclaredStatic = DeclaredPublic | BindingFlags.Static;
        private const BindingFlags DeclaredInstance = DeclaredPublic | BindingFlags.Instance;
        private const BindingFlags DeclaredPublic = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly | BindingFlags.ExactBinding;

        private static readonly System.Type DelegateType = typeof(System.Delegate);
        internal static readonly System.Type DynamicInvocableType = typeof(IDynamicInvocable);

        public static ConstructorInfo GetInstanceCtor(this System.Type type, params System.Type[] parameterTypes)
        {
            var result = type.GetConstructor(DeclaredInstance, null, parameterTypes, null);
            if (result == null)
                throw new System.InvalidOperationException(string.Format("the ctor {0}.ctor({1})", type.FullName, StringHelpers.Join(Separator, parameterTypes)));
            return result;
        }

        public static MethodInfo GetStaticMethod(this System.Type type, string name, params System.Type[] parameterTypes)
        {
            MethodInfo result = type.GetMethod(name, DeclaredStatic, null, parameterTypes, null);
            if (result == null)
                throw new System.InvalidOperationException(string.Format("the static method {0}.{1}({2})", type.FullName, name, StringHelpers.Join(Separator, parameterTypes)));
            return result;
        }

        public static MethodInfo GetImplicitConversion(this System.Type type, string name, System.Type returnType, params System.Type[] parameterTypes)
        {
            var results = type.GetMember(name, MemberTypes.Method, DeclaredStatic);
            foreach (MethodInfo method in results)
            {
                if (method.MatchesArgumentTypes(parameterTypes) && TypeUtils.AreReferenceAssignable(method.ReturnType, returnType))
                {
                    return method;
                }
            }
            throw new System.InvalidOperationException(string.Format("the convertion method {0}.{1}({2})", type.FullName, name, StringHelpers.Join(Separator, parameterTypes)));
        }

        public static FieldInfo GetField(this System.Type type, string name, BindingFlags binding)
        {
            FieldInfo result = type.GetField(name, binding);
            if (result == null)
                throw new System.InvalidOperationException(string.Format("the field {0}.{1}", type.FullName, name));
            return result;

        }

        public static MethodInfo GetInstanceMethod(this System.Type type, string name, params System.Type[] parameterTypes)
        {
            MethodInfo result = type.GetMethod(name, DeclaredInstance, null, parameterTypes, null);
            if (result == null)
                throw new System.InvalidOperationException(string.Format("The instance method {0}.{1}({2}) does not exist.", type.FullName, name, StringHelpers.Join(", ", parameterTypes)));
            return result;
        }

        public static System.TypeCode FindTypeCode(this System.Type type)
        {
            switch (type.Name)
            {
                case "Byte":
                    return System.TypeCode.SByte;
                case "Char":
                    return System.TypeCode.Char;
                case "Short":
                case "Int16":
                    return System.TypeCode.Int16;
                case "Int32":
                case "Integer":
                    return System.TypeCode.Int32;
                case "Float":
                case "Single":
                    return System.TypeCode.Single;
                case "Double":
                    return System.TypeCode.Double;
                case "Boolean":
                    return System.TypeCode.Boolean;
                default:
                    return System.Type.GetTypeCode(type);
            }
        }

        public static MethodInfo FindMethod(this System.Type type, string name, System.Type[] types, BindingFlags bindingAttr, out ArgumentConversions conversions)
        {
            conversions = new ArgumentConversions(types.Length);
            if (type.IsDefined(typeof(RegisterAttribute), false))
            {
                var methods = type.GetMethods(bindingAttr);
                for (int i = 0; i < methods.Length; i++)
                {
                    var m = methods[i];
                    var attrs = (System.Attribute[])m.GetCustomAttributes(typeof(RegisterAttribute), false);
                    if (attrs.Length > 0 && attrs[0].Match(name)
                        && m.MatchesArgumentTypes(types, conversions))
                        return m;
                }
                return null;
            }

            return FindSystemMethod(type, name, types, bindingAttr, conversions);
        }

        public static MethodInfo FindMethod(this System.Type type, string name, System.Type[] types, BindingFlags bindingAttr = TypeUtils.AnyPublic)
        {
            if (type.IsDefined(typeof(RegisterAttribute), false))
            {
                var methods = type.GetMethods(bindingAttr);
                for (int i = 0; i < methods.Length; i++)
                {
                    var m = methods[i];
                    var attrs = (System.Attribute[])m.GetCustomAttributes(typeof(RegisterAttribute), false);
                    if (attrs.Length > 0 && attrs[0].Match(name)
                        && m.MatchesArgumentTypes(types))
                        return m;
                }
                return null;
            }

            return FindSystemMethod(type, name, types);
        }

        public static bool TryFindMember(this System.Type type, string name, BindingFlags flags, out IBinder binder)
        {
            if (type == null)
            {
                binder = null;
                return false;
            }
            if (type.IsDefined(typeof(RegisterAttribute), false))
            {
                return FindMember(type, name, flags, out binder);
            }
            binder = FindSystemMember(type, name, flags);
            return binder != null;
        }

        public static bool FindMember(this System.Type type, string name, BindingFlags flags, out IBinder binder)
        {
            if (type != null)
            {
                var properties = type.GetProperties(flags);
                for (int i = 0; i < properties.Length; i++)
                {
                    var p = properties[i];
                    var data = (System.Attribute[])p.GetCustomAttributes(typeof(RegisterAttribute), false);
                    if (data.Length > 0 && data[0].Match(name))
                    {
                        binder = new PropertyBinder(p);
                        return true;
                    }
                }

                var fields = type.GetFields(flags);
                for (int i = 0; i < fields.Length; i++)
                {
                    var f = fields[i];
                    var data = (System.Attribute[])f.GetCustomAttributes(typeof(RegisterAttribute), false);
                    if (data.Length > 0 && data[0].Match(name))
                    {
                        binder = new FieldBinder(f);
                        return true;
                    }
                }
                return FindMember(type.BaseType, name, flags, out binder);
            }
            binder = null;
            return false;
        }

        public static IBinder FindSystemMember(this System.Type type, string name, BindingFlags flags)
        {
            if (type != null)
            {
                var p = type.GetProperty(name, flags | BindingFlags.IgnoreCase);
                if (p != null)
                    return new PropertyBinder(p);
                var f = type.GetField(name, flags);
                if (f != null)
                    return new FieldBinder(f);
                return FindSystemMember(type.BaseType, name, flags);
            }
            return null;
        }

        public static MethodInfo FindSystemMethod(this System.Type type, string name, System.Type[] types, BindingFlags bindingAttr = ReflectionUtils.AnyPublic)
        {
            var members = type.GetMember(name, MemberTypes.Method, bindingAttr);
            // start from last ex: in Console.Write(String) no match at the beginning
            for (int i = members.Length - 1; i >= 0; i--)
            {
                MethodInfo m = (MethodInfo)members[i];
                if (m.MatchesArgumentTypes(types))
                    return m;
            }
            return null;
        }

        public static MethodInfo FindSystemMethod(this System.Type type, string name, System.Type[] types, BindingFlags bindingAttr, ArgumentConversions conversions)
        {
            var members = type.GetMember(name, MemberTypes.Method, bindingAttr);
            // start from last ex: in Console.Write(String) no match at the beginning
            for (int i = members.Length - 1; i >= 0; i--)
            {
                MethodInfo m = (MethodInfo)members[i];
                if (m.MatchesArgumentTypes(types, conversions))
                    return m;
            }
            return null;
        }

        #region Indexer
        /// Current Declared Indexer can get
        public static MethodInfo FindGetIndexer(this System.Type type, System.Type[] types, out ArgumentConversions conversions)
        {
            conversions = new ArgumentConversions(types.Length);
            if (type.IsArray)
            {
                var m = type.GetMethod("Get", TypeUtils.PublicInstance);
                //for array no indexer we have to use Get method
                if (m.MatchesArgumentTypes(types, conversions))
                {
                    return m;
                }
            }
            foreach (var item in type.GetDefaultMembers())
            {
                if (item.MemberType == MemberTypes.Property)
                {
                    var p = (PropertyInfo)item;
                    if (p.CanRead)
                    {
                        var m = p.GetGetMethod(true);
                        if (m.MatchesArgumentTypes(types, conversions))
                        {
                            return m;
                        }
                    }
                }
            }
            return null;
        }
        #endregion

        #region Delegate Type
        public static bool IsDelegate(this System.Type type)
        {
            if (type == null)
                return false;
            return DelegateType.IsAssignableFrom(type);
        }
        #endregion

        #region Dynamic Invocable Type
        public static bool IsDynamicInvocable(this System.Type type)
        {
            if (type == null)
                return false;
            return DynamicInvocableType.IsAssignableFrom(type);
        }
        #endregion

    }
}
