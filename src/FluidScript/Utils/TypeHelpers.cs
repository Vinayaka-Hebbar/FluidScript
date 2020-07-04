using FluidScript.Runtime;
using System.Reflection;

namespace FluidScript.Utils
{
    public static class TypeHelpers
    {
        public static TMethod BindToMethod<TMethod>(TMethod[] methods, object[] args, out ArgumentConversions conversions) where TMethod : MethodBase
        {
            conversions = new ArgumentConversions(args.Length);
            foreach (var m in methods)
            {
                if (TypeUtils.MatchesTypes(m, args, conversions))
                    return m;
            }
            return null;
        }

        internal static MethodInfo BindToMethod(MemberInfo[] members, object[] args, out ArgumentConversions conversions)
        {
            conversions = new ArgumentConversions(args.Length);
            foreach (var m in members)
            {
                if (m.MemberType == MemberTypes.Method)
                {
                    if (TypeUtils.MatchesTypes((MethodInfo)m, args, conversions))
                        return (MethodInfo)m;
                }
            }
            return null;
        }

        internal static MethodInfo GetDelegateMethod(System.Delegate del, object[] args, out ArgumentConversions conversions)
        {
            conversions = new ArgumentConversions(args.Length);
            MethodInfo m = del.Method;
            // only static method can allowed
            if (TypeUtils.MatchesTypes(m, args, conversions))
            {
                return m;
            }
            return null;
        }
    }
}
