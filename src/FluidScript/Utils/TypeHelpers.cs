using FluidScript.Compiler.Binders;
using System.Reflection;

namespace FluidScript.Utils
{
    internal static class TypeHelpers
    {
        internal static TMethod BindToMethod<TMethod>(TMethod[] methods, object[] args, out ArgumentConversions conversions) where TMethod : MethodBase
        {
            conversions = new ArgumentConversions(args.Length);
            foreach (var m in methods)
            {
                if (MatchesTypes(m, args, conversions))
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
                    if (MatchesTypes((MethodInfo)m, args, conversions))
                        return (MethodInfo)m;
                }
            }
            return null;
        }

        internal static bool MatchesTypes(MethodBase method, object[] args, ArgumentConversions conversions)
        {
            var parameters = method.GetParameters();
            // arg length
            var length = args.Length;
            // no arg
            if (parameters.Length == 0 && length > 0)
                return false;
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
            if (i == length)
                return true;
            return conversions.Recycle();
        }

        private static bool ParamArrayMatchs(System.Collections.IList args, int index, System.Type dest, ArgumentConversions conversions)
        {
            var binder = new ArgumentConversions(args.Count);
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
                        binder.Add(new ParamConversion(i, opImplict));
                    }
                }
            }
            conversions.Add(new ParamArrayConversion(index, dest, binder));
            return true;
        }

        internal static MethodInfo FindMethod(string name, System.Type type, object[] args, out ArgumentConversions conversions)
        {
            conversions = new ArgumentConversions(args.Length);
            bool isRuntime = type.IsDefined(typeof(Runtime.RegisterAttribute), false);
            if (!isRuntime)
                return FindSystemMethod(name, type, args, conversions);
            var methods = type.GetMethods(TypeUtils.AnyPublic);
            for (int i = 0; i < methods.Length; i++)
            {
                var m = methods[i];
                var attrs = (Runtime.RegisterAttribute[])m.GetCustomAttributes(typeof(Runtime.RegisterAttribute), false);
                if (attrs.Length > 0 && attrs[0].Match(name)
                    && MatchesTypes(m, args, conversions))
                    return m;
            }
            return null;
        }

        private static MethodInfo FindSystemMethod(string name, System.Type type, object[] args, ArgumentConversions conversions)
        {
            foreach (MethodInfo m in type.GetMember(name, MemberTypes.Method, TypeUtils.AnyPublic))
            {
                if (MatchesTypes(m, args, conversions))
                    return m;
            }
            return null;
        }

        internal static MethodInfo GetDelegateMethod(System.Delegate del, object[] args, out ArgumentConversions conversions)
        {
            conversions = new ArgumentConversions(args.Length);
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
