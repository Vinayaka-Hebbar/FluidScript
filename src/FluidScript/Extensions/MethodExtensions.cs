using FluidScript.Runtime;
using System;
using System.Reflection;

namespace FluidScript.Extensions
{
    public static class MethodExtensions
    {
        public static bool MatchesArgumentTypes(this MethodBase method, Type[] types)
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
                    if (dest.IsValueType && !dest.IsNullableType())
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

        public static bool MatchesArgumentTypes(this MethodBase method, Type[] types, ArgumentConversions conversions)
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
                    if (dest.IsValueType && !dest.IsNullableType())
                        return conversions.Recycle();
                }
                else if (!TypeUtils.AreReferenceAssignable(dest, src))
                {
                    if (src.TryImplicitConvert(dest, out MethodInfo m) == false)
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
                    if (dest.IsValueType && !dest.IsNullableType())
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
                    if (dest.IsValueType && !dest.IsNullableType())
                        return false;
                }
                else if (!TypeUtils.AreReferenceAssignable(dest, src))
                {
                    if (src.TryImplicitConvert(dest, out MethodInfo opImplict) == false)
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
    }
}
