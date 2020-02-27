using FluidScript.Compiler.Binders;
using System.Linq;
using System.Reflection;

namespace FluidScript.Utils
{
    internal static class TypeHelpers
    {
        internal static MethodInfo BindToMethod(MethodInfo[] methods, object[] args, out ArgumentBinderList bindings)
        {
            bindings = new ArgumentBinderList();
            foreach (var m in methods)
            {
                if (MatchesTypes(m, args, ref bindings))
                    return m;
            }
            return null;
        }

        internal static MethodInfo BindToMethod(MemberInfo[] members, System.Collections.IList agrs, out ArgumentBinderList bindings)
        {
            bindings = new ArgumentBinderList();
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

        internal static bool MatchesTypes(MethodInfo method, System.Collections.IList args, ref ArgumentBinderList bindings)
        {
            var paramters = method.GetParameters();
            // arg length
            var length = args.Count;
            // no arg 
            if (paramters.Length == 0 && length > 0)
                return false;
            bindings.Clear();
            for (int i = 0; i < paramters.Length; i++)
            {
                var param = paramters[i];
                var dest = param.ParameterType;
                if (param.IsDefined(typeof(System.ParamArrayAttribute), false))
                {
                    // no arg ok
                    bindings.Add(new ParamArrayBinder(i, param.ParameterType));
                    //No further check required
                    break;
                }
                // matches current index
                if (i >= length)
                    return false;
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

        internal static bool MatchesTypes(System.Type[] types, System.Collections.IList args, ref ArgumentBinderList bindings)
        {
            bindings.Clear();
            var length = args.Count;
            if (types.Length == 0 && length > 0)
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

        internal static bool HasMember(MemberInfo m, object filter)
        {
            var data = (System.Attribute)m.GetCustomAttributes(typeof(Runtime.RegisterAttribute), false).FirstOrDefault();
            return data != null ? data.Match(filter) : m.Name.Equals(filter);
        }

        internal static MemberInfo GetMember(object obj, string name)
        {
            if (obj == null)
                return null;
            var members = obj.GetType().FindMembers(MemberTypes.Field | MemberTypes.Property, TypeUtils.PublicInstance, HasMember, name);
            if (members.Length > 0)
                return members[0];
            return null;
        }

        internal static MethodInfo[] GetPublicMethods(object obj, string name)
        {
            if (obj == null)
                return new MethodInfo[0];
            var type = obj.GetType();
            return new ArrayFilterIterator<MethodInfo>(type.GetMethods(TypeUtils.Any), TypeUtils.HasMethod, name).ToArray();
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

        internal static MethodInfo GetDelegateMethod(System.Delegate del, ref object[] args, out ArgumentBinderList binds)
        {
            binds = new ArgumentBinderList();
            MethodInfo m = del.Method;
            // only static method can allowed
            if (del.Target is Runtime.Function function)
            {
                if (MatchesTypes(function.ParameterTypes, args, ref binds))
                {
                    args = new object[] { args };
                    return m;
                }
            }
            else
            {
                m = del.Method;
                if (MatchesTypes(m, args, ref binds))
                {
                    return m;
                }
            }
            return null;
        }
    }
}
