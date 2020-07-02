using FluidScript.Runtime;
using System.Reflection;

namespace FluidScript.Utils
{
    public static class TypeHelpers
    {
        public static TMethod BindToMethod<TMethod>(TMethod[] methods, object[] args, out ArgumentConversions conversions) where TMethod : MethodBase
        {
            conversions = new ArgumentConversions(args.Length);
            foreach (var m in methods)
            {
                if (TypeUtils.MatchesTypes(m, args, conversions))
                    return m;
            }
            return null;
        }

        internal static MethodInfo BindToMethod(MemberInfo[] members, object[] args, out ArgumentConversions conversions)
        {
            conversions = new ArgumentConversions(args.Length);
            foreach (var m in members)
            {
                if (m.MemberType == MemberTypes.Method)
                {
                    if (TypeUtils.MatchesTypes((MethodInfo)m, args, conversions))
                        return (MethodInfo)m;
                }
            }
            return null;
        }

        public static bool TryFindMethod(string name, System.Type type, object[] args, out MethodInfo method, out ArgumentConversions conversions)
        {
            conversions = new ArgumentConversions(args.Length);
            return type.IsInterface
                ? TryFindInterfaceMethod(name, type, args, out method, conversions)
                : type.IsDefined(typeof(RegisterAttribute), false)
                ? FindMethods(name, type, ReflectionUtils.AnyPublic, args, out method, conversions)
                : TryFindSystemMethod(name, type, ReflectionUtils.AnyPublic, args, out method, conversions);
        }

        private static bool FindMethods(string name, System.Type type, BindingFlags flags, object[] args, out MethodInfo method, ArgumentConversions conversions)
        {
            if (type != null)
            {
                var methods = type.GetMethods(flags);
                for (int i = 0; i < methods.Length; i++)
                {
                    var m = methods[i];
                    var attrs = (Runtime.RegisterAttribute[])m.GetCustomAttributes(typeof(Runtime.RegisterAttribute), false);
                    if (attrs.Length > 0 && attrs[0].Match(name)
                        && TypeUtils.MatchesTypes(m, args, conversions))
                    {
                        method = m;
                        return true;
                    }
                }
                return FindMethods(name, type.BaseType, ReflectionUtils.PublicStatic, args, out method, conversions);
            }
            method = null;
            return false;
        }

        static bool TryFindInterfaceMethod(string name, System.Type type, object[] args, out MethodInfo method, ArgumentConversions conversions)
        {
            if (TryFindSystemMethod(name, type, ReflectionUtils.PublicInstance, args, out method, conversions))
                return true;
            var types = type.GetInterfaces();
            for (int i = 0; i < types.Length; i++)
            {
                type = types[i];
                if (TryFindSystemMethod(name, type, ReflectionUtils.PublicInstance, args, out method, conversions))
                    return true;
            }
            return false;
        }

        private static bool TryFindSystemMethod(string name, System.Type type, BindingFlags flags, object[] args, out MethodInfo method, ArgumentConversions conversions)
        {
            if (type != null)
            {
                foreach (MethodInfo m in type.GetMethods(flags))
                {
                    if (m.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase) && TypeUtils.MatchesTypes(m, args, conversions))
                    {
                        method = m;
                        return true;
                    }
                }
            }

            method = null;
            return false;
        }

        internal static MethodInfo GetDelegateMethod(System.Delegate del, object[] args, out ArgumentConversions conversions)
        {
            conversions = new ArgumentConversions(args.Length);
            MethodInfo m = del.Method;
            // only static method can allowed
            if (TypeUtils.MatchesTypes(m, args, conversions))
            {
                return m;
            }
            return null;
        }

        #region Indexer
        /// Current Declared Indexer can get
        public static MethodInfo FindGetIndexer(this System.Type type, object[] args, out ArgumentConversions conversions)
        {
            conversions = new ArgumentConversions(args.Length);
            if (type.IsArray)
            {
                //for array no indexer
                var m = type.GetMethod("Get", ReflectionUtils.PublicInstance);
                if (TypeUtils.MatchesTypes(m, args, conversions))
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
                        if (TypeUtils.MatchesTypes(m, args, conversions))
                        {
                            return m;
                        }
                    }
                }
            }
            return null;
        }

        /// Current Declared Indexer can get
        public static MethodInfo FindSetIndexer(this System.Type type, object[] args, out ArgumentConversions conversions)
        {
            conversions = new ArgumentConversions(args.Length);
            if (type.IsArray)
            {
                //for array no indexer
                var m = type.GetMethod("Set", ReflectionUtils.PublicInstance);
                if (TypeUtils.MatchesTypes(m, args, conversions))
                {
                    return m;
                }
            }
            foreach (var item in type.GetDefaultMembers())
            {
                if (item.MemberType == MemberTypes.Property)
                {
                    var p = (PropertyInfo)item;
                    if (p.CanWrite)
                    {
                        var m = p.GetSetMethod(true);
                        if (TypeUtils.MatchesTypes(m, args, conversions))
                        {
                            return m;
                        }
                    }
                }
            }
            return null;
        }
        #endregion
    }
}
