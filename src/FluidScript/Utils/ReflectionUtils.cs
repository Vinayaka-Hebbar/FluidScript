using FluidScript.Compiler;
using FluidScript.Extensions;
using FluidScript.Runtime;
using System;
using System.Reflection;

namespace FluidScript.Utils
{
    public static class ReflectionUtils
    {
        internal const string ParseMethod = "Parse";
        internal const string InvokeMethod = "Invoke";

        #region Types
        internal const string ConvertibleType = "System.IConvertible";
        #endregion

        internal const BindingFlags PublicInstance = BindingFlags.Public | BindingFlags.Instance;
        internal const BindingFlags PublicStatic = BindingFlags.Public | BindingFlags.Static;
        internal const BindingFlags AnyPublic = PublicStatic | BindingFlags.Instance;
        internal const BindingFlags Any = AnyPublic | BindingFlags.NonPublic;
        internal const BindingFlags PublicInstanceDeclared = PublicInstance | BindingFlags.DeclaredOnly;
        internal const BindingFlags PublicDeclared = PublicInstanceDeclared | BindingFlags.Static;

        #region BindToMethod
        public static MethodInfo BindToMethod(MemberInfo[] members, Type[] types, out ArgumentConversions bindings)
        {
            bindings = new ArgumentConversions(types.Length);
            foreach (var m in members)
            {
                if (m.MemberType == MemberTypes.Method)
                {
                    if (MethodExtensions.MatchesArgumentTypes((MethodInfo)m, types, bindings))
                        return (MethodInfo)m;
                }
            }
            return null;
        }

        public static TMethod BindToMethod<TMethod>(TMethod[] methods, Type[] types, out ArgumentConversions bindings) where TMethod : MethodBase
        {
            bindings = new ArgumentConversions(types.Length);
            foreach (var m in methods)
            {
                if (m.MatchesArgumentTypes(types, bindings))
                    return m;
            }
            return null;
        }

        public static TMethod BindToMethod<TMethod>(TMethod[] methods, object[] args, out ArgumentConversions conversions) where TMethod : MethodBase
        {
            conversions = new ArgumentConversions(args.Length);
            foreach (var m in methods)
            {
                if (m.MatchesArguments(args, conversions))
                    return m;
            }
            return null;
        }

        public static MethodInfo BindToMethod(MemberInfo[] members, object[] args, out ArgumentConversions conversions)
        {
            conversions = new ArgumentConversions(args.Length);
            foreach (var m in members)
            {
                if (m.MemberType == MemberTypes.Method)
                {
                    if (ReflectionExtensions.MatchesArguments((MethodInfo)m, args, conversions))
                        return (MethodInfo)m;
                }
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

        internal static Conversion[] FromSystemType(ref Type[] types)
        {
            Conversion[] conversions = new Conversion[types.Length];
            for (int i = 0; i < types.Length; i++)
            {
                var type = types[i];
                if (type.IsPrimitive)
                {
                    var typeCode = Type.GetTypeCode(type);
                    conversions[i] = new ParamConversion(i, ReflectionHelpers.ToAny);
                    types[i] = TypeProvider.Find(typeCode);
                }
            }
            return conversions;
        }

        public static MethodInfo GetOperatorOverload(string name, ArgumentConversions conversions, params Type[] types)
        {
            for (int i = 0; i < types.Length; i++)
            {
                var members = types[i].GetMember(name, PublicStatic);
                foreach (MethodInfo m in members)
                {
                    if (MethodExtensions.MatchesArgumentTypes(m, types, conversions))
                        return m;
                }
            }
            return null;
        }

        internal static bool BindingFlagsMatch(bool state, BindingFlags flags, BindingFlags trueFlag, BindingFlags falseFlag)
        {
            return (state && (flags & trueFlag) == trueFlag)
                || (!state && (flags & falseFlag) == falseFlag);

        }

        internal static MethodInfo GetImplicitToBooleanOperator(Type type)
        {
            if (type == TypeProvider.BooleanType)
            {
                return null;
            }
            if (type.IsPrimitive && type == typeof(bool))
            {
                return TypeProvider.BooleanType.GetMethod(TypeUtils.ImplicitConversionName, PublicStatic, null, new Type[1] { type }, null);
            }
            else if (type.GetInterface(ConvertibleType, false) != null)
            {
                return ReflectionHelpers.ToBoolean;
            }
            var methods = type.GetMember(TypeUtils.ImplicitConversionName, MemberTypes.Method, PublicStatic);
            foreach (MethodInfo method in methods)
            {
                if (method.MatchesArgumentTypes(type) && method.ReturnType == TypeProvider.BooleanType)
                    return method;
            }
            throw new Exception(string.Concat("can't convert from ", type, " to type Boolean"));
        }

        #region Delegate

        internal static bool TryGetDelegateMethod(object obj, object[] args, out MethodInfo method, out ArgumentConversions conversions)
        {
            conversions = new ArgumentConversions(args.Length);
            method = obj.GetType().GetMethod(InvokeMethod, PublicInstance);
            // only static method can allowed
            return method.MatchesArguments(args, conversions);
        }

        public static bool TryGetDelegateMethod(Type type, Type[] args, out MethodInfo method, out ArgumentConversions conversions)
        {
            conversions = new ArgumentConversions(args.Length);
            method = type.GetMethod(InvokeMethod, PublicInstance);
            // only static method can allowed
            return method.MatchesArgumentTypes(args, conversions);
        }
        #endregion

    }
}
