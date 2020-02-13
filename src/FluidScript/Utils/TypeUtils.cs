
using FluidScript.Reflection;
using FluidScript.Reflection.Emit;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FluidScript.Utils
{
    internal static class TypeUtils
    {
        internal static readonly IList<Primitive> Inbuilts;

        internal const string ImplicitConversionName = "op_Implicit";

        internal const string ExplicitConviersionName = "op_Explicit";

        #region Types
        internal static readonly System.Type BooleanType = typeof(Boolean);
        internal static readonly System.Type FSType = typeof(FSObject);
        internal static readonly System.Type ObjectType = typeof(object);
        private const string ConvertibleType = "System.IConvertible";
        #endregion

        private static readonly IDictionary<string, Primitive> InbuiltNames;
        internal const BindingFlags Any = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;
        internal const BindingFlags PublicStatic = BindingFlags.Public | BindingFlags.Static;
        internal const BindingFlags DeclaredPublic = BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly;

        /// <summary>
        /// Must be primitive
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static Primitive From(string typeName)
        {
            if (InbuiltNames.ContainsKey(typeName))
                return InbuiltNames[typeName];
            return Primitive.Any;
        }

        static TypeUtils()
        {
            Inbuilts = new List<Primitive>
            {
                new Primitive("byte", typeof(Byte)),
                new Primitive("short", typeof(Short)),
                new Primitive("int", typeof(Integer)),
                new Primitive("long", typeof(Long)),
                new Primitive("float", typeof(Float)),
                new Primitive("double", typeof(Double)),
                new Primitive("bool", BooleanType),
                new Primitive("char", typeof(Char)),
                new Primitive("string", typeof(String)),
                new Primitive("any", typeof(IFSObject)),
                new Primitive("void", typeof(void))
            };
            InbuiltNames = Inbuilts.ToDictionary(item => item.Name);
        }

        internal static bool HasType(string name)
        {
            return InbuiltNames.ContainsKey(name);
        }

        internal static System.Type GetType(string typeName)
        {
            if (InbuiltNames.ContainsKey(typeName))
                return InbuiltNames[typeName].Type;
            return System.Type.GetType(typeName);
        }

        internal static System.Type GetInbuiltType(string typeName)
        {
            return InbuiltNames[typeName].Type;
        }

        internal static bool IsInbuiltType(string typeName)
        {
            return InbuiltNames.ContainsKey(typeName);
        }

        internal static bool TryGetType(TypeName typeName, out System.Type type)
        {
            if (InbuiltNames.TryGetValue(typeName.FullName, out Primitive inbuilt))
            {
                type = inbuilt.Type;
                return true;
            }
            type = null;
            return false;
        }

        #region BindToMethod
        internal static MethodInfo BindToMethod(MemberInfo[] members, System.Type[] types, out ParamBindList bindings)
        {
            bindings = new Reflection.Emit.ParamBindList();
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

        internal static MethodInfo BindToMethod(MethodInfo[] methods, System.Type[] types, out ParamBindList bindings)
        {
            bindings = new ParamBindList();
            foreach (var m in methods)
            {
                if (MatchesTypes(m, types, ref bindings))
                    return m;
            }
            return null;
        }

        #endregion

        internal static MethodInfo GetOperatorOverload(string name, out ParamBindList bindings, params System.Type[] types)
        {
            bindings = new ParamBindList();
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

        internal static bool MatchesTypes(MethodInfo method, System.Type[] types, ref ParamBindList bindings)
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
                    bindings.Add(new ParamArrayBind(i, param.ParameterType));
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
                    if (TryImplicitConvert(src, dest, out MethodInfo opImplict) == false)
                        return false;
                    bindings.Add(new ParamConvert(i, opImplict));
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
            // todo base class convert
            var methods = dest.GetMember(ImplicitConversionName, MemberTypes.Method, PublicStatic);
            foreach (MethodInfo m in methods)
            {
                if (MatchesArgumentTypes(m, src) && AreReferenceAssignable(m.ReturnType, dest))
                {
                    method = m;
                    return true;
                }
            }
            methods = src.GetMember(ImplicitConversionName, MemberTypes.Method, PublicStatic);
            foreach (MethodInfo m in methods)
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
            if (type == BooleanType)
            {
                return null;
            }
            if (type.IsPrimitive && type == typeof(bool))
            {
                return BooleanType.GetMethod(ImplicitConversionName, PublicStatic, null, new System.Type[1] { type }, null);
            }
            else if (type.GetInterface(ConvertibleType, false) != null)
            {
                return Helpers.ToBoolean;
            }
            var methods = type.GetMember(ImplicitConversionName, MemberTypes.Method, PublicStatic);
            foreach (MethodInfo method in methods)
            {
                if (MatchesArgumentTypes(method, type) && method.ReturnType == BooleanType)
                    return method;
            }
            throw new System.Exception(string.Concat("can't convert from ", type, " to type Boolean"));
        }
    }
}
