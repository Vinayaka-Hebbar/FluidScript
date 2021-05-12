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
                        // if exceeds index
                        if (i >= length)
                            continue;
                        var param = paramters[i];
                        var dest = param.ParameterType;
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

        public static bool AreReferenceAssignable(this Type dest, Type src)
        {
            if (src is null)
                return false;
            if (ReferenceEquals(src, dest))
                return true;
            // if both are value type types are not assignable
            if (src.IsValueType && dest.IsValueType)
                return false;
            // WARNING: This actually implements "Is this identity assignable and/or reference assignable?"
            // if src is TypeBuilder 
            if (src.IsSubclassOf(dest))
                return true;
            if (dest.IsInterface)
                return dest.ImplementInterface(src);
            if (dest.IsGenericParameter)
            {
                var constraints = dest.GetGenericParameterConstraints();
                for (int i = 0; i < constraints.Length; i++)
                {
                    if (!constraints[i].IsAssignableFrom(src))
                        return false;
                }
                return true;
            }
            return dest.IsAssignableFrom(src);
        }
    }
}
