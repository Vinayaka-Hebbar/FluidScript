using FluidScript.Compiler.Binders;
using System.Reflection;

namespace FluidScript.Utils
{
    internal static class TypeHelpers
    {
        internal static MethodInfo BindToMethod(MethodInfo[] methods, object[] args, out ArgumenConversions conversions)
        {
            conversions = new ArgumenConversions(args.Length);
            foreach (var m in methods)
            {
                if (MatchesTypes(m, args, conversions))
                    return m;
            }
            return null;
        }

        internal static MethodInfo BindToMethod(MemberInfo[] members, System.Collections.IList args, out ArgumenConversions conversions)
        {
            conversions = new ArgumenConversions(args.Count);
            foreach (var m in members)
            {
                if (m.MemberType == MemberTypes.Method)
                {
                    if (MatchesTypes((MethodInfo)m, args, conversions))
                        return (MethodInfo)m;
                }
            }
            return null;
        }

        internal static bool MatchesTypes(MethodInfo method, System.Collections.IList args, ArgumenConversions conversions)
        {
            var parameters = method.GetParameters();
            // arg length
            var length = args.Count;
            // no arg
            if (parameters.Length == 0 && length > 0)
                return false;
            conversions.Clear();
            int i;
            for (i = 0; i < parameters.Length; i++)
            {
                var param = parameters[i];
                var dest = param.ParameterType;
                if (param.IsDefined(typeof(System.ParamArrayAttribute), false))
                {
                    // parameters is extra example print(string, params string[] args) and print('hello')
                    // in this case 2 and 1
                    if (parameters.Length > length)
                    {
                        conversions.Add(new ParamArrayConversion(i, dest.GetElementType()));
                        return true;
                    }
                    //No further check required if matchs
                    return ParamArrayMatchs(args, i, dest.GetElementType(), conversions);
                }
                // matches current index
                if (i >= length)
                    return false;
                var arg = args[i];
                if (arg is null)
                {
                    if (dest.IsValueType && !TypeUtils.IsNullableType(dest))
                        return false;
                }
                else
                {
                    var src = arg.GetType();
                    if (!TypeUtils.AreReferenceAssignable(dest, src))
                    {
                        if (TypeUtils.TryImplicitConvert(src, dest, out MethodInfo opImplict) == false)
                            return false;
                        conversions.Add(new ParamConversion(i, opImplict));
                    }
                }
            }
            return i == length;
        }

        private static bool ParamArrayMatchs(System.Collections.IList args, int index, System.Type dest, ArgumenConversions conversions)
        {
            var binder = new ArgumenConversions();
            // check first parameter type matches
            for (var i = index; i < args.Count; i++)
            {
                var arg = args[i];
                if (arg is null)
                {
                    if (dest.IsValueType && !TypeUtils.IsNullableType(dest))
                        return false;
                }
                else
                {
                    var src = arg.GetType();
                    if (!TypeUtils.AreReferenceAssignable(dest, src))
                    {
                        if (TypeUtils.TryImplicitConvert(src, dest, out MethodInfo opImplict) == false)
                            return false;
                        conversions.Add(new ParamConversion(i, opImplict));
                    }
                }
            }
            conversions.Add(new ParamArrayConversion(index, dest, binder));
            return true;
        }

        internal static bool MatchesTypes(System.Type[] types, System.Collections.IList args, ArgumenConversions conversions)
        {
            var length = args.Count;
            if (types.Length == 0 && length > 0)
                return false;
            conversions.Clear();
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
                    conversions.Add(new ParamConversion(i, opImplict));
                }
            }
            return true;
        }

        internal static MethodInfo[] GetPublicMethods(object obj, string name)
        {
            if (obj == null)
                return new MethodInfo[0];
            return TypeUtils.GetPublicMethods(obj.GetType(), name);
        }

        internal static object InvokeSet(MemberInfo m, object obj, object value, out System.Type type)
        {
            if (m.MemberType == MemberTypes.Field)
            {
                var f = (FieldInfo)m;
                if (f.IsInitOnly)
                    throw new System.MemberAccessException(string.Concat("cannot write to readonly field ", f.Name));
                type = f.FieldType;
                f.SetValue(obj, value);
            }
            else if (m.MemberType == MemberTypes.Property)
            {
                var p = (PropertyInfo)m;
                if (!p.CanWrite)
                    throw new System.MemberAccessException(string.Concat("cannot write to readonly property ", p.Name));
                if (value != null && p.PropertyType != value.GetType())
                {
                    if (TypeUtils.TryImplicitConvert(value.GetType(), p.PropertyType, out MethodInfo implictCast))
                    {
                        value = implictCast.Invoke(null, new object[1] { value });
                    }
                }
                type = p.PropertyType;
                p.SetValue(obj, value, new object[0]);
            }
            else
            {
                throw new System.MemberAccessException(string.Concat("cannot write to member", m.Name));
            }
            return value;
        }

        internal static object InvokeGet(MemberInfo m, object obj, out System.Type type)
        {
            if (m.MemberType == MemberTypes.Field)
            {
                var f = (FieldInfo)m;
                type = f.FieldType;
                return f.GetValue(obj);
            }
            else if (m.MemberType == MemberTypes.Property)
            {
                var p = (PropertyInfo)m;
                if (!p.CanRead)
                    throw new System.MemberAccessException(string.Concat("cannot read to property", m.Name));
                type = p.PropertyType;
                return p.GetValue(obj, new object[0]);
            }
            else if (m.MemberType == MemberTypes.Method)
            {
                var method = (MethodInfo)m;
                type = method.ReturnType;
                return method.Invoke(obj, new object[0]);
            }
            throw new System.MemberAccessException(string.Concat("cannot read to member", m.Name));
        }

        internal static MethodInfo GetDelegateMethod(System.Delegate del, object[] args, out ArgumenConversions conversions)
        {
            conversions = new ArgumenConversions();
            MethodInfo m = del.Method;
            // only static method can allowed
            if (MatchesTypes(m, args, conversions))
            {
                return m;
            }
            return null;
        }
    }
}
