using FluidScript.Compiler;
using FluidScript.Compiler.Binders;
using System.Linq;
using System.Reflection;

namespace FluidScript.Utils
{
    internal static class TypeUtils
    {

        internal const string ImplicitConversionName = "op_Implicit";

        internal const string ExplicitConviersionName = "op_Explicit";

        #region Types
        private const string ConvertibleType = "System.IConvertible";
        #endregion

        internal const BindingFlags Any = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;
        internal const BindingFlags PublicStatic = BindingFlags.Public | BindingFlags.Static;
        internal const BindingFlags PublicDeclared = PublicInstance | BindingFlags.Static | BindingFlags.DeclaredOnly;
        internal const BindingFlags PublicInstance = BindingFlags.Public | BindingFlags.Instance;

        static TypeUtils()
        {
        }

        #region BindToMethod
        internal static MethodInfo BindToMethod(MemberInfo[] members, System.Type[] types, out ArgumentBinderList bindings)
        {
            bindings = new ArgumentBinderList();
            foreach (var m in members)
            {
                if (m.MemberType == MemberTypes.Method)
                {
                    if (MatchesTypes((MethodInfo)m, types, ref bindings))
                        return (MethodInfo)m;
                }
            }
            return null;
        }

        internal static MethodInfo BindToMethod(MethodInfo[] methods, System.Type[] types, out ArgumentBinderList bindings)
        {
            bindings = new ArgumentBinderList();
            foreach (var m in methods)
            {
                if (MatchesTypes(m, types, ref bindings))
                    return m;
            }
            return null;
        }

        #endregion

        internal static MethodInfo GetOperatorOverload(string name, out ArgumentBinderList bindings, params System.Type[] types)
        {
            bindings = new ArgumentBinderList();
            foreach (var type in types)
            {
                var members = type.GetMember(name, PublicStatic);
                foreach (MethodInfo m in members)
                {
                    if (MatchesTypes(m, types, ref bindings))
                        return m;
                }
            }
            return null;
        }

        internal static bool MatchesTypes(MethodInfo method, System.Type[] types, ref ArgumentBinderList bindings)
        {
            var paramters = method.GetParameters();
            var length = types.Length;
            if (paramters.Length < length)
                return false;
            // clear previous bindings
            bindings.Clear();
            for (int i = 0; i < paramters.Length; i++)
            {
                var param = paramters[i];
                if (param.IsDefined(typeof(System.ParamArrayAttribute), false))
                {
                    bindings.Add(new ParamArrayBinder(i, param.ParameterType));
                    //No further check required
                    break;
                }
                // matches current index
                if (i >= length)
                    return false;
                var dest = param.ParameterType;
                var src = types[i];
                if (!AreReferenceAssignable(dest, src))
                {
                    if (TryImplicitConvert(src, dest, out MethodInfo m) == false)
                        return false;
                    bindings.Add(new ParamConvert(i, m));
                }
            }
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
        internal static bool MatchesArgumentTypes(MethodInfo m, params System.Type[] argTypes)
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


        internal static bool AreReferenceAssignable(System.Type dest, System.Type src)
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

        internal static bool IsNullAssignable(System.Type type)
        {
            return type.IsValueType == false || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(System.Nullable<>));
        }

        internal static bool IsNullableType(System.Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(System.Nullable<>);
        }

        internal static bool TryImplicitConvert(System.Type src, System.Type dest, out MethodInfo method)
        {
            // todo base class convert check
            var members = dest.GetMember(ImplicitConversionName, MemberTypes.Method, PublicStatic);
            foreach (MethodInfo m in members)
            {
                if (MatchesArgumentTypes(m, src) && AreReferenceAssignable(m.ReturnType, dest))
                {
                    method = m;
                    return true;
                }
            }
            members = src.GetMember(ImplicitConversionName, MemberTypes.Method, PublicStatic);
            foreach (MethodInfo m in members)
            {
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

        #region Methods

        internal static bool HasMethod(MethodInfo m, object filter)
        {
            var data = (System.Attribute)m.GetCustomAttributes(typeof(Runtime.RegisterAttribute), false).FirstOrDefault();
            return data != null ? data.Match(filter) : m.Name.Equals(filter);
        }

        internal static MethodInfo[] GetPublicMethods(System.Type type, string name)
        {
            if (type == null)
                return new MethodInfo[0];
            return new ArrayFilterIterator<MethodInfo>(type.GetMethods(Any), HasMethod, name).ToArray();
        }
        #endregion
    }
}
