using FluidScript.Reflection.Emit;
using System.Reflection;

namespace FluidScript.Utils
{
    internal static class DynamicUtils
    {

        internal static MethodInfo BindToMethod(MethodInfo[] methods, object[] args, out ParamBindList bindings)
        {
            bindings = new ParamBindList();
            foreach (var m in methods)
            {
                if (MatchesTypes(m, args, ref bindings))
                    return m;
            }
            return null;
        }


        internal static MethodInfo BindToMethod(MemberInfo[] members, System.Collections.IList agrs, out ParamBindList bindings)
        {
            bindings = new ParamBindList();
            foreach (var m in members)
            {
                if (m.MemberType == MemberTypes.Method)
                {
                    if (MatchesTypes((MethodInfo)m, agrs, ref bindings))
                        return (MethodInfo)m;
                }
            }
            return null;
        }



        internal static bool MatchesTypes(MethodInfo method, System.Collections.IList args, ref ParamBindList bindings)
        {
            var paramters = method.GetParameters();
            // arg length
            var length = args.Count;
            // no arg or less
            if (paramters.Length < length)
                return false;
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
                var arg = args[i];
                if (arg == null)
                {
                    //for value type if nullable
                    if (dest.IsValueType && !TypeUtils.IsNullableType(dest))
                        return false;
                    else
                        continue;
                }
                var src = arg.GetType();
                if (!TypeUtils.AreReferenceAssignable(dest, src))
                {
                    if (TypeUtils.TryImplicitConvert(src, dest, out MethodInfo opImplict) == false)
                        return false;
                    bindings.Add(new ParamConvert(i, opImplict));
                }
            }
            return true;
        }

        internal static bool MatchesTypes(System.Type[] types, System.Collections.IList args, ref ParamBindList bindings)
        {
            bindings.Clear();
            var length = args.Count;
            if (types.Length < length)
                return false;
            // arg length
            for (int i = 0; i < types.Length; i++)
            {
                // matches current index
                if (i >= length)
                    return false;
                var dest = types[i];
                var arg = args[i];
                if (arg == null)
                {
                    //for value type if nullable
                    if (dest.IsValueType && !TypeUtils.IsNullableType(dest))
                        return false;
                    else
                        continue;
                }
                var src = arg.GetType();
                if (!TypeUtils.AreReferenceAssignable(dest, src))
                {
                    if (TypeUtils.TryImplicitConvert(src, dest, out MethodInfo opImplict) == false)
                        return false;
                    bindings.Add(new ParamConvert(i, opImplict));
                }
            }
            return true;
        }
    }
}
