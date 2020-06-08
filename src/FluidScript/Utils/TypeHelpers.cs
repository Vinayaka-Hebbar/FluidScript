using FluidScript.Compiler.Binders;
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
                if (MatchesTypes(m, args, conversions))
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
                    if (MatchesTypes((MethodInfo)m, args, conversions))
                        return (MethodInfo)m;
                }
            }
            return null;
        }

        internal static bool MatchesTypes(MethodBase method, object[] args, ArgumentConversions conversions)
        {
            var parameters = method.GetParameters();
            // arg length
            var length = args.Length;
            // no arg
            if (parameters.Length == 0 && length > 0)
                return false;
            int i;
            for (i = 0; i < parameters.Length; i++)
            {
                var param = parameters[i];
                var dest = param.ParameterType;
                if (param.IsDefined(typeof(System.ParamArrayAttribute), false))
                {
                    // parameters is extra example print(string, params string[] args) and print('hello')
                    // in this case 2 and 1
                    if (parameters.Length > length)
                    {
                        conversions.Add(new ParamArrayConversion(i, dest.GetElementType()));
                        return true;
                    }
                    //No further check required if matchs
                    return ParamArrayMatchs(args, i, dest.GetElementType(), conversions);
                }
                // matches current index
                if (i >= length)
                    return false;
                var arg = args[i];
                if (arg is null)
                {
                    if (dest.IsValueType && !TypeUtils.IsNullableType(dest))
                        return false;
                }
                else
                {
                    var src = arg.GetType();
                    if (!TypeUtils.AreReferenceAssignable(dest, src))
                    {
                        if (TypeUtils.TryImplicitConvert(src, dest, out MethodInfo opImplict) == false)
                            return false;
                        conversions.Add(new ParamConversion(i, opImplict));
                    }
                }
            }
            if (i == length)
                return true;
            return conversions.Recycle();
        }

        private static bool ParamArrayMatchs(System.Collections.IList args, int index, System.Type dest, ArgumentConversions conversions)
        {
            var binder = new ArgumentConversions(args.Count);
            // check first parameter type matches
            for (var i = index; i < args.Count; i++)
            {
                var arg = args[i];
                if (arg is null)
                {
                    if (dest.IsValueType && !TypeUtils.IsNullableType(dest))
                        return false;
                }
                else
                {
                    var src = arg.GetType();
                    if (!TypeUtils.AreReferenceAssignable(dest, src))
                    {
                        if (TypeUtils.TryImplicitConvert(src, dest, out MethodInfo opImplict) == false)
                            return false;
                        binder.Add(new ParamConversion(i, opImplict));
                    }
                }
            }
            conversions.Add(new ParamArrayConversion(index, dest, binder));
            return true;
        }

        public static bool TryFindMethod(string name, System.Type type, object[] args, out MethodInfo method, out ArgumentConversions conversions)
        {
            conversions = new ArgumentConversions(args.Length);
            return type.IsInterface
                ? TryFindInterfaceMethod(name, type, args, out method, conversions)
                : type.IsDefined(typeof(Runtime.RegisterAttribute), false)
                ? FindMethods(name, type, TypeUtils.AnyPublic, args, out method, conversions)
                : TryFindSystemMethod(name, type, TypeUtils.AnyPublic, args, out method, conversions);
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
                        && MatchesTypes(m, args, conversions))
                    {
                        method = m;
                        return true;
                    }
                }
                return FindMethods(name, type.BaseType, TypeUtils.PublicStatic, args, out method, conversions);
            }
            method = null;
            return false;
        }

        static bool TryFindInterfaceMethod(string name, System.Type type, object[] args, out MethodInfo method, ArgumentConversions conversions)
        {
            if (TryFindSystemMethod(name, type, TypeUtils.PublicInstance, args, out method, conversions))
                return true;
            var types = type.GetInterfaces();
            for (int i = 0; i < types.Length; i++)
            {
                type = types[i];
                if (TryFindSystemMethod(name, type, TypeUtils.PublicInstance, args, out method, conversions))
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
                    if (m.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase) && MatchesTypes(m, args, conversions))
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
            if (MatchesTypes(m, args, conversions))
            {
                return m;
            }
            return null;
        }

        /// Current Declared Indexer can get
        public static MethodInfo FindGetIndexer(this System.Type type, object[] args, out ArgumentConversions conversions)
        {
            conversions = new ArgumentConversions(args.Length);
            if (type.IsArray)
            {
                //for array no indexer
                var m = type.GetMethod("Get", TypeUtils.PublicInstance);
                if (MatchesTypes(m, args, conversions))
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
                        if (MatchesTypes(m, args, conversions))
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
                var m = type.GetMethod("Set", TypeUtils.PublicInstance);
                if (MatchesTypes(m, args, conversions))
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
                        if (MatchesTypes(m, args, conversions))
                        {
                            return m;
                        }
                    }
                }
            }
            return null;
        }
    }
}
