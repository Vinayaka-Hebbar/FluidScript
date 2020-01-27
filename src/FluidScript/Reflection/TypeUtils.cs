
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FluidScript.Reflection
{
    public static class TypeUtils
    {
        internal static readonly IList<Primitive> Inbuilts;


        #region Types
        internal static readonly System.Type BooleanType = typeof(Boolean);
        internal static readonly System.Type ObjectType = typeof(object);
        #endregion

        private static readonly Emit.Conversion[] NoConversions = new Emit.Conversion[0];

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
                new Primitive("bool", typeof(Boolean)),
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

        internal static MethodInfo BindToMethod(MemberInfo[] members, System.Type[] types, out Emit.Conversion[] conversions)
        {
            foreach (var m in members)
            {
                if (m.MemberType == MemberTypes.Method)
                {
                    if (MatchesTypes((MethodInfo)m, types, out conversions))
                        return (MethodInfo)m;
                }
            }
            conversions = new Emit.Conversion[0];
            return null;
        }

        internal static MethodInfo BindToMethod(MethodInfo[] methods, System.Type[] types, out Emit.Conversion[] conversions)
        {
            foreach (var m in methods)
            {
                if (MatchesTypes(m, types, out conversions))
                    return m;
            }
            conversions = new Emit.Conversion[0];
            return null;
        }

        internal static MethodInfo GetOperatorOverload(string name, out Emit.Conversion[] conversions, params System.Type[] types)
        {
            foreach (var type in types)
            {
                var members = type.GetMember(name, PublicStatic);
                foreach (MethodInfo m in members)
                {
                    if (MatchesTypes(m, types, out conversions))
                        return m;
                }
            }
            conversions = NoConversions;
            return null;
        }

        internal static bool MatchesTypes(MethodInfo method, System.Type[] types, out Emit.Conversion[] conversions)
        {
            var paramters = method.GetParameters();
            if (paramters.Length != types.Length)
            {
                conversions = null;
                return false;
            }
            conversions = new Emit.Conversion[paramters.Length];
            for (int i = 0; i < paramters.Length; i++)
            {
                var dest = paramters[i].ParameterType;
                var src = types[i];
                if (dest.IsAssignableFrom(src))
                    conversions[i] = Emit.Conversion.NoConversion;
                else
                {
                    if (TryImplicitConvert(src, dest, out MethodInfo opImplict) == false)
                        return false;
                    conversions[i] = new Emit.Conversion(opImplict);
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

        internal static bool TryImplicitConvert(System.Type src, System.Type dest, out MethodInfo method)
        {
            // todo base class convert
            var methods = dest.GetMember(Emit.Conversion.ImplicitConversionName, MemberTypes.Method, PublicStatic);
            foreach (MethodInfo m in methods)
            {
                if (MatchesArgumentTypes(m, src) && AreReferenceAssignable(m.ReturnType, dest))
                {
                    method = m;
                    return true;
                }
            }
            methods = src.GetMember(Emit.Conversion.ImplicitConversionName, MemberTypes.Method, PublicStatic);
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
            return type.IsPrimitive && type == typeof(bool)
                ? BooleanType.GetMethod(Emit.Conversion.ImplicitConversionName, PublicStatic, null, new System.Type[1] { type }, null)
                : type.GetMethod(Emit.Conversion.ImplicitConversionName, PublicStatic, null, new System.Type[1] { type }, null);
        }
    }
}
