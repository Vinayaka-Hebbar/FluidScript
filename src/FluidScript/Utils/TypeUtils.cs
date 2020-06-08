using FluidScript.Compiler;
using FluidScript.Compiler.Binders;
using System.Reflection;

namespace FluidScript.Utils
{
    internal static class TypeUtils
    {

        internal const string ImplicitConversionName = "op_Implicit";
        internal const string ExplicitConviersionName = "op_Explicit";
        internal const string ParseMethod = "Parse";

        #region Types
        private const string ConvertibleType = "System.IConvertible";
        #endregion

        internal const BindingFlags PublicInstance = BindingFlags.Public | BindingFlags.Instance;
        internal const BindingFlags PublicStatic = BindingFlags.Public | BindingFlags.Static;
        internal const BindingFlags AnyPublic = PublicStatic | BindingFlags.Instance;
        internal const BindingFlags Any = AnyPublic | BindingFlags.NonPublic;
        internal const BindingFlags PublicDeclared = PublicInstance | BindingFlags.Static | BindingFlags.DeclaredOnly;

        #region BindToMethod
        internal static MethodInfo BindToMethod(MemberInfo[] members, System.Type[] types, out ArgumentConversions bindings)
        {
            bindings = new ArgumentConversions(types.Length);
            foreach (var m in members)
            {
                if (m.MemberType == MemberTypes.Method)
                {
                    if (MatchesTypes((MethodInfo)m, types, bindings))
                        return (MethodInfo)m;
                }
            }
            return null;
        }

        internal static TMethod BindToMethod<TMethod>(TMethod[] methods, System.Type[] types, out ArgumentConversions bindings) where TMethod : MethodBase
        {
            bindings = new ArgumentConversions(types.Length);
            foreach (var m in methods)
            {
                if (MatchesTypes(m, types, bindings))
                    return m;
            }
            return null;
        }

        internal static MethodInfo FindMethod(string name, System.Type type, System.Type[] types, out ArgumentConversions conversions)
        {
            conversions = new ArgumentConversions(types.Length);
            bool isRuntime = type.IsDefined(typeof(Runtime.RegisterAttribute), false);
            if (!isRuntime)
                return FindSystemMethod(name, type, types, conversions);
            var methods = type.GetMethods(AnyPublic);
            for (int i = 0; i < methods.Length; i++)
            {
                var m = methods[i];
                var attrs = (Runtime.RegisterAttribute[])m.GetCustomAttributes(typeof(Runtime.RegisterAttribute), false);
                if (attrs.Length > 0 && attrs[0].Match(name)
                    && MatchesTypes(m, types, conversions))
                    return m;
            }
            return null;
        }

        private static MethodInfo FindSystemMethod(string name, System.Type type, System.Type[] types, ArgumentConversions conversions)
        {
            foreach (MethodInfo m in type.GetMember(name, MemberTypes.Method, AnyPublic))
            {
                if (MatchesTypes(m, types, conversions))
                    return m;
            }
            return null;
        }

        #endregion

#if Experiment
        private static MethodInfo ValueConvert(System.Type src, System.Type desc)
        {
            var i = desc.GetInterface("IValueBox`1");
            if (i != null)
            {
                var type = i.GetGenericArguments()[0];
                if (AreReferenceAssignable(src, type))
                {
                    return desc.GetMethod(ParseMethod, PublicStatic);
                }
            }
            return null;
        }
#endif

        internal static void FromSystemType(ArgumentConversions conversions, ref System.Type[] types)
        {
            for (int i = 0; i < types.Length; i++)
            {
                var type = types[i];
                if (type.IsPrimitive)
                {
                    var typeCode = System.Type.GetTypeCode(type);
                    conversions.Insert(i, new ParamConversion(i, ReflectionHelpers.ToAny));
                    types[i] = TypeProvider.Find(typeCode);
                }
            }
            conversions.Backup();
        }

        public static MethodInfo GetOperatorOverload(string name, ArgumentConversions conversions, params System.Type[] types)
        {
            for (int i = 0; i < types.Length; i++)
            {
                var members = types[i].GetMember(name, PublicStatic);
                foreach (MethodInfo m in members)
                {
                    if (MatchesTypes(m, types, conversions))
                        return m;
                }
            }
            return null;
        }

        public static bool MatchesTypes(MethodBase method, System.Type[] types, ArgumentConversions conversions)
        {
            var paramters = method.GetParameters();
            var length = types.Length;
            if (paramters.Length < length)
                return false;
            int i;
            for (i = 0; i < paramters.Length; i++)
            {
                var param = paramters[i];
                var dest = param.ParameterType;
                if (param.IsDefined(typeof(System.ParamArrayAttribute), false))
                {
                    conversions.Add(new ParamArrayConversion(i, dest.GetElementType()));
                    //No further check required
                    return true;
                }
                // matches current index
                if (i >= length)
                    return conversions.Recycle();
                var src = types[i];
                if (!AreReferenceAssignable(dest, src))
                {
                    if (TryImplicitConvert(src, dest, out MethodInfo m) == false)
                        return conversions.Recycle();
                    conversions.Add(new ParamConversion(i, m));
                }
            }
            if (i == length)
                return true;
            return conversions.Recycle();
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
        public static bool MatchesArgumentTypes(MethodInfo m, params System.Type[] argTypes)
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

        public static bool AreReferenceAssignable(System.Type dest, System.Type src)
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

        public static bool IsNullAssignable(System.Type type)
        {
            return type.IsValueType == false || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(System.Nullable<>));
        }

        internal static bool IsNullableType(System.Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(System.Nullable<>);
        }

        public static bool TryImplicitConvert(System.Type src, System.Type dest, out MethodInfo method)
        {
            //if (src.IsPrimitive && dest.IsPrimitive == false && dest.IsValueType)
            //{
            //    method = ValueConvert(src, dest);
            //    if (method != null)
            //        return true;
            //}
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

        public static bool TryExplicitConvert(System.Type src, System.Type dest, out MethodInfo method)
        {
            //if (src.IsPrimitive && dest.IsPrimitive == false && dest.IsValueType)
            //{
            //    method = ValueConvert(src, dest);
            //    if (method != null)
            //        return true;
            //}
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

        internal static bool BindingFlagsMatch(bool state, BindingFlags flags, BindingFlags trueFlag, BindingFlags falseFlag)
        {
            return (state && (flags & trueFlag) == trueFlag)
                || (!state && (flags & falseFlag) == falseFlag);

        }

        internal static MethodInfo GetBooleanOveraload(System.Type type)
        {
            if (type == TypeProvider.BooleanType)
            {
                return null;
            }
            if (type.IsPrimitive && type == typeof(bool))
            {
                return TypeProvider.BooleanType.GetMethod(ImplicitConversionName, PublicStatic, null, new System.Type[1] { type }, null);
            }
            else if (type.GetInterface(ConvertibleType, false) != null)
            {
                return ReflectionHelpers.ToBoolean;
            }
            var methods = type.GetMember(ImplicitConversionName, MemberTypes.Method, PublicStatic);
            foreach (MethodInfo method in methods)
            {
                if (MatchesArgumentTypes(method, type) && method.ReturnType == TypeProvider.BooleanType)
                    return method;
            }
            throw new System.Exception(string.Concat("can't convert from ", type, " to type Boolean"));
        }

        #region Member

        public static bool TryFindMember(System.Type type, string name, BindingFlags flags, out IBinder binder)
        {
            bool isRuntime = type.IsDefined(typeof(Runtime.RegisterAttribute), false);
            if (isRuntime)
            {
                return FindMember(type, name, flags, out binder);
            }
            binder = FindSystemMember(type, name, flags);
            return binder != null;
        }

        public static bool FindMember(System.Type type, string name, BindingFlags flags, out IBinder binder)
        {
            if (type != null)
            {
                var properties = type.GetProperties(flags);
                for (int i = 0; i < properties.Length; i++)
                {
                    var p = properties[i];
                    var data = (System.Attribute[])p.GetCustomAttributes(typeof(Runtime.RegisterAttribute), false);
                    if (data.Length > 0 && data[0].Match(name))
                    {
                        binder = new PropertyBinder(p);
                        return true;
                    }
                }

                var fields = type.GetFields(AnyPublic);
                for (int i = 0; i < fields.Length; i++)
                {
                    var f = fields[i];
                    var data = (System.Attribute[])f.GetCustomAttributes(typeof(Runtime.RegisterAttribute), false);
                    if (data.Length > 0 && data[0].Match(name))
                    {
                        binder = new FieldBinder(f);
                        return true;
                    }
                }
                return FindMember(type.BaseType, name, PublicStatic, out binder);
            }
            binder = null;
            return false;
        }

        public static IBinder FindSystemMember(System.Type type, string name, BindingFlags flags)
        {
            if (type != null)
            {
                var p = type.GetProperty(name, flags | BindingFlags.IgnoreCase);
                if (p != null)
                    return new PropertyBinder(p);
                var f = type.GetField(name, flags);
                if (f != null)
                    return new FieldBinder(f);
                return FindSystemMember(type.BaseType, name, PublicStatic);
            }
            return null;
        }
        #endregion
    }
}
