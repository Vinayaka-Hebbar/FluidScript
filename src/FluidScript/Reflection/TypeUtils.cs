using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FluidScript.Reflection
{
    public static class TypeUtils
    {
        internal static readonly IList<Primitive> Inbuilts;
        private static readonly IDictionary<string, Primitive> InbuiltNames;
        internal const BindingFlags Any = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;

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
                new Primitive("any", typeof(FSObject)),
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

        internal static MethodInfo BindToMethod(MethodInfo[] methods, System.Type[] types, out Emit.Conversion[] conversions)
        {
            foreach (var m in methods)
            {
                if (MatchTypes(m, types, out conversions))
                    return m;
            }
            conversions = new Emit.Conversion[0];
            return null;
        }

        internal static MethodInfo GetOperatorOverload(string name, out Emit.Conversion[] conversions, params System.Type[] types)
        {
            foreach (var type in types)
            {
                var members = type.GetMember(name, BindingFlags.Public | BindingFlags.Static);
                foreach (MethodInfo m in members)
                {
                    if (MatchTypes(m, types, out conversions))
                        return m;
                }
            }
            throw new System.Exception("No operator overload");
        }

        private static bool MatchTypes(MethodInfo method, System.Type[] types, out Emit.Conversion[] conversions)
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
                var expected = paramters[i].ParameterType;
                var type = types[i];
                if (expected == type)
                    conversions[i] = Emit.Conversion.NoConversion;
                else
                {
                    if (HasImplicitConvert(type, expected, out MethodInfo opImplict) == false)
                        return false;
                    conversions[i] = new Emit.Conversion(opImplict);
                }
            }
            return true;
        }

        internal static bool TryImplicitConvert(System.Type from, System.Type to, out MethodInfo method)
        {
            method = to.GetMethod(Emit.Conversion.ImplicitConversionName, BindingFlags.Public | BindingFlags.Static, null, new System.Type[1] { from }, null);
            if (method != null && method.ReturnType == to)
                return true;
            method = from.GetMethod(Emit.Conversion.ImplicitConversionName, BindingFlags.Public | BindingFlags.Static, null, new System.Type[1] { from }, null);
            return method != null && method.ReturnType == to;
        }

        internal static bool HasImplicitConvert(System.Type from, System.Type to, out MethodInfo method)
        {
            method = to.GetMethod(Emit.Conversion.ImplicitConversionName, BindingFlags.Public | BindingFlags.Static, null, new System.Type[1] { from }, null);
            if (method != null && method.ReturnType == to)
                return true;
            method = from.GetMethod(Emit.Conversion.ImplicitConversionName, BindingFlags.Public | BindingFlags.Static, null, new System.Type[1] { from }, null);
            return method != null && method.ReturnType == to;
        }

        internal static bool BindingFlagsMatch(bool state, BindingFlags flags, BindingFlags trueFlag, BindingFlags falseFlag)
        {
            return (state && (flags & trueFlag) == trueFlag)
                || (!state && (flags & falseFlag) == falseFlag);

        }
    }
}
