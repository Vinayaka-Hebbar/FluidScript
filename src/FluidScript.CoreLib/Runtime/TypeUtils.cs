using System;
using System.Reflection;

namespace FluidScript.Runtime
{
    public static class TypeUtils
    {
        public const BindingFlags PublicStatic = BindingFlags.Public | BindingFlags.Static;
        public const BindingFlags AnyPublic = PublicStatic | BindingFlags.Instance;
        public const BindingFlags PublicInstance = BindingFlags.Public | BindingFlags.Instance;

        public const string ImplicitConversionName = "op_Implicit";
        public const string ExplicitConviersionName = "op_Explicit";

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
                            if (src.TryImplicitConvert(dest, out MethodInfo m))
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
    }
}
