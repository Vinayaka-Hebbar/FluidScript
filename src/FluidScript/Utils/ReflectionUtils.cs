using FluidScript.Compiler;
using FluidScript.Compiler.Binders;
using FluidScript.Runtime;
using System;
using System.Reflection;

namespace FluidScript.Utils
{
    internal static class ReflectionUtils
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
        internal const BindingFlags PublicInstanceDeclared = PublicInstance | BindingFlags.DeclaredOnly;
        internal const BindingFlags PublicDeclared = PublicInstanceDeclared | BindingFlags.Static;

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
                    conversions.Append(i, new ParamConversion(i, ReflectionHelpers.ToAny));
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

        public static bool MatchesTypes(MethodBase method, Type[] types)
        {
            var parameters = method.GetParameters();
            var length = types.Length;
            if (parameters.Length < length)
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
                        return true;
                    }
                    //No further check required if matchs
                    return ParamArrayMatchs(types, i, dest.GetElementType());
                }
                // matches current index
                if (i >= length)
                    return false;
                var src = types[i];
                if (src is null)
                {
                    if (dest.IsValueType && !TypeUtils.IsNullableType(dest))
                        return false;
                }
                else if (!TypeUtils.AreReferenceAssignable(dest, src))
                {
                    return false;
                }
            }
            if (i == length)
                return true;
            return false;
        }

        public static bool MatchesTypes(MethodBase method, Type[] types, ArgumentConversions conversions)
        {
            var parameters = method.GetParameters();
            var length = types.Length;
            if (parameters.Length < length)
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
                    return ParamArrayMatchs(types, i, dest.GetElementType(), conversions);
                }
                // matches current index
                if (i >= length)
                    return conversions.Recycle();
                var src = types[i];
                if (src is null)
                {
                    if (dest.IsValueType && !TypeUtils.IsNullableType(dest))
                        return conversions.Recycle();
                }
                else if (!TypeUtils.AreReferenceAssignable(dest, src))
                {
                    if (TypeUtils.TryImplicitConvert(src, dest, out MethodInfo m) == false)
                        return conversions.Recycle();
                    if (src.IsValueType && m.GetParameters()[0].ParameterType.IsValueType == false)
                        conversions.Add(new BoxConversion(i, src));
                    conversions.Add(new ParamConversion(i, m));
                }
                if (src.IsValueType && dest.IsValueType == false)
                {
                    conversions.Add(new BoxConversion(i, src));
                }
            }
            if (i == length)
                return true;
            return conversions.Recycle();
        }

        static bool ParamArrayMatchs(Type[] types, int index, Type dest)
        {
            // check first parameter type matches
            for (; index < types.Length; index++)
            {
                var src = types[index];
                if (src is null)
                {
                    if (dest.IsValueType && !TypeUtils.IsNullableType(dest))
                        return false;
                }
                else if (!TypeUtils.AreReferenceAssignable(dest, src))
                {
                    return false;
                }
            }
            return true;
        }

        static bool ParamArrayMatchs(Type[] types, int index, Type dest, ArgumentConversions conversions)
        {
            var binder = new ArgumentConversions(types.Length - index);
            // check first parameter type matches
            for (int i = 0, current = index; current < types.Length; i++, current++)
            {
                var src = types[current];
                if (src is null)
                {
                    if (dest.IsValueType && !TypeUtils.IsNullableType(dest))
                        return false;
                }
                else if (!TypeUtils.AreReferenceAssignable(dest, src))
                {
                    if (TypeUtils.TryImplicitConvert(src, dest, out MethodInfo opImplict) == false)
                        return false;
                    if (src.IsValueType && opImplict.GetParameters()[0].ParameterType.IsValueType == false)
                        binder.Add(new BoxConversion(i, src));
                    binder.Add(new ParamConversion(i, opImplict));
                }
                else if (src.IsValueType && dest.IsValueType == false)
                {
                    conversions.Add(new BoxConversion(i, src));
                }
            }
            conversions.Add(new ParamArrayConversion(index, dest, binder));
            return true;
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
                if (TypeUtils.MatchesArgumentTypes(method, type) && method.ReturnType == TypeProvider.BooleanType)
                    return method;
            }
            throw new Exception(string.Concat("can't convert from ", type, " to type Boolean"));
        }

        #region Member


        #endregion
    }
}
