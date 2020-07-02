using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace FluidScript.Runtime
{
    public static class TypeUtils
    {
        internal const BindingFlags PublicStatic = BindingFlags.Public | BindingFlags.Static; 
        internal const BindingFlags AnyPublic = PublicStatic | BindingFlags.Instance; 
        internal const BindingFlags PublicInstance = BindingFlags.Public | BindingFlags.Instance;
        internal const string ImplicitConversionName = "op_Implicit";
        internal const string ExplicitConviersionName = "op_Explicit";

        public static MethodInfo GetOperatorOverload(string name, Type[] types, out MethodInfo[] conversions)
        {
            int length = types.Length;
            conversions = new MethodInfo[length];
            for (int index = 0; index < length; index++)
            {
                var methods = (MethodInfo[])types[index].GetMember(name, MemberTypes.Method, PublicStatic);
                foreach (var method in methods)
                {
                    var paramters = method.GetParameters();
                    if (paramters.Length < length)
                        continue;
                    int i;
                    for (i = 0; i < paramters.Length; i++)
                    {
                        var param = paramters[i];
                        var dest = param.ParameterType;
                        // matches current index
                        if (i >= length)
                            continue;
                        var src = types[i];
                        if (!AreReferenceAssignable(dest, src))
                        {
                            if (TryImplicitConvert(src, dest, out MethodInfo m))
                            {
                                conversions[i] = m;
                            }
                            else
                            {
                                throw new InvalidCastException("Unable to cast from " + src + " to type " + dest);
                            }
                        }
                        else
                        {
                            conversions[i] = null;
                        }
                    }
                    if (i == length)
                        return method;
                }
            }
            return null;
        }

        public static bool MatchesTypes(MethodBase method, object[] args, ArgumentConversions conversions)
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
                if (param.IsDefined(typeof(ParamArrayAttribute), false))
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
                    break;
                var arg = args[i];
                if (arg is null)
                {
                    if (dest.IsValueType && !IsNullableType(dest))
                        break;
                }
                else
                {
                    var src = arg.GetType();
                    if (!AreReferenceAssignable(dest, src))
                    {
                        if (TryImplicitConvert(src, dest, out MethodInfo opImplict) == false)
                            break;
                        conversions.Add(new ParamConversion(i, opImplict));
                    }
                }
            }
            if (i == length)
                return true;
            return conversions.Recycle();
        }

        static bool ParamArrayMatchs(System.Collections.IList args, int index, Type dest, ArgumentConversions conversions)
        {
            var binder = new ArgumentConversions(args.Count);
            // check first parameter type matches
            for (var i = index; i < args.Count; i++)
            {
                var arg = args[i];
                if (arg is null)
                {
                    if (dest.IsValueType && !IsNullableType(dest))
                        return false;
                }
                else
                {
                    var src = arg.GetType();
                    if (!AreReferenceAssignable(dest, src))
                    {
                        if (TryImplicitConvert(src, dest, out MethodInfo opImplict) == false)
                            return false;
                        binder.Add(new ParamConversion(i, opImplict));
                    }
                }
            }
            conversions.Add(new ParamArrayConversion(index, dest, binder));
            return true;
        }

        /// <summary>
        /// Returns true if the method's parameter types are reference assignable from
        /// the argument types, otherwise false.
        /// 
        /// An example that can make the method return false is that 
        /// typeof(double).GetMethod("op_Equality", ..., new[] { typeof(double), typeof(int) })
        /// returns a method with two double parameters, which doesn't match the provided
        /// argument types.
        /// </summary>
        /// <returns></returns>
        public static bool MatchesArgumentTypes(MethodInfo m, params Type[] argTypes)
        {
            if (m == null || argTypes == null)
            {
                return false;
            }
            var ps = m.GetParameters();

            if (ps.Length != argTypes.Length)
            {
                return false;
            }

            for (int i = 0; i < ps.Length; i++)
            {
                if (!AreReferenceAssignable(ps[i].ParameterType, argTypes[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool AreReferenceAssignable(Type dest, Type src)
        {
            // WARNING: This actually implements "Is this identity assignable and/or reference assignable?"
            if (dest.IsAssignableFrom(src))
            {
                return true;
            }
            if (!dest.IsValueType && !src.IsValueType && dest.IsAssignableFrom(src))
            {
                return true;
            }
            return false;
        }

        public static bool IsNullAssignable(Type type)
        {
            return type.IsValueType == false || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(System.Nullable<>));
        }

        public static bool IsNullableType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(System.Nullable<>);
        }

        public static bool TryImplicitConvert(Type src, Type dest, out MethodInfo method)
        {
            // todo base class convert check
            var methods = (MethodInfo[])src.GetMember(ImplicitConversionName, MemberTypes.Method, PublicStatic);
            for (int i = 0; i < methods.Length; i++)
            {
                MethodInfo m = methods[i];
                if (MatchesArgumentTypes(m, src) && AreReferenceAssignable(m.ReturnType, dest))
                {
                    method = m;
                    return true;
                }
            }
            methods = (MethodInfo[])dest.GetMember(ImplicitConversionName, MemberTypes.Method, PublicStatic);
            for (int i = 0; i < methods.Length; i++)
            {
                MethodInfo m = methods[i];
                if (MatchesArgumentTypes(m, src) && AreReferenceAssignable(m.ReturnType, dest))
                {
                    method = m;
                    return true;
                }
            }
            method = null;
            return false;
        }

        public static bool TryExplicitConvert(Type src, Type dest, out MethodInfo method)
        {
            // todo base class convert check
            var methods = (MethodInfo[])src.GetMember(ExplicitConviersionName, MemberTypes.Method, PublicStatic);
            for (int i = 0; i < methods.Length; i++)
            {
                MethodInfo m = methods[i];
                if (MatchesArgumentTypes(m, src) && AreReferenceAssignable(m.ReturnType, dest))
                {
                    method = m;
                    return true;
                }
            }
            methods = (MethodInfo[])dest.GetMember(ExplicitConviersionName, MemberTypes.Method, PublicStatic);
            for (int i = 0; i < methods.Length; i++)
            {
                MethodInfo m = methods[i];
                if (MatchesArgumentTypes(m, src) && AreReferenceAssignable(m.ReturnType, dest))
                {
                    method = m;
                    return true;
                }
            }
            method = null;
            return false;
        }

        #region Find Method
        public static bool TryFindMethod(string name, Type type, object[] args, out MethodInfo method, out ArgumentConversions conversions)
        {
            conversions = new ArgumentConversions(args.Length);
            return type.IsInterface
                ? TryFindInterfaceMethod(name, type, args, out method, conversions)
                : type.IsDefined(typeof(RegisterAttribute), false)
                ? FindMethods(name, type, AnyPublic, args, out method, conversions)
                : TryFindSystemMethod(name, type, AnyPublic, args, out method, conversions);
        }

        private static bool FindMethods(string name, Type type, BindingFlags flags, object[] args, out MethodInfo method, ArgumentConversions conversions)
        {
            if (type != null)
            {
                var methods = type.GetMethods(flags);
                for (int i = 0; i < methods.Length; i++)
                {
                    var m = methods[i];
                    var attrs = (RegisterAttribute[])m.GetCustomAttributes(typeof(RegisterAttribute), false);
                    if (attrs.Length > 0 && attrs[0].Match(name)
                        && MatchesTypes(m, args, conversions))
                    {
                        method = m;
                        return true;
                    }
                }
                return FindMethods(name, type.BaseType, PublicStatic, args, out method, conversions);
            }
            method = null;
            return false;
        }

        static bool TryFindInterfaceMethod(string name, Type type, object[] args, out MethodInfo method, ArgumentConversions conversions)
        {
            if (TryFindSystemMethod(name, type, PublicInstance, args, out method, conversions))
                return true;
            var types = type.GetInterfaces();
            for (int i = 0; i < types.Length; i++)
            {
                type = types[i];
                if (TryFindSystemMethod(name, type, PublicInstance, args, out method, conversions))
                    return true;
            }
            return false;
        }

        private static bool TryFindSystemMethod(string name, Type type, BindingFlags flags, object[] args, out MethodInfo method, ArgumentConversions conversions)
        {
            if (type != null)
            {
                foreach (MethodInfo m in type.GetMethods(flags))
                {
                    if (m.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && MatchesTypes(m, args, conversions))
                    {
                        method = m;
                        return true;
                    }
                }
            }

            method = null;
            return false;
        }
        #endregion
    }
}
