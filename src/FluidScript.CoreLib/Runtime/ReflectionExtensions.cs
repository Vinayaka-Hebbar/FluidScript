using System;
using System.Reflection;

namespace FluidScript.Runtime
{
    public static class ReflectionExtensions
    {
        public static bool MatchesArguments(this MethodBase method, object[] args, ArgumentConversions conversions)
        {
            var parameters = method.GetParameters();
            // arg length
            var length = args.Length;
            // no arg
            int argCount = parameters.Length;
            if (argCount == 0 && length > 0)
                return false;
            int i;
            for (i = 0; i < argCount; i++)
            {
                var param = parameters[i];
                var dest = param.ParameterType;
                if (param.IsDefined(typeof(ParamArrayAttribute), false))
                {
                    // parameters is extra example print(string, params string[] args) and print('hello')
                    // in this case 2 and 1
                    if (argCount > length)
                    {
                        conversions.Add(new ParamArrayConversion(i, dest.GetElementType()));
                        return true;
                    }
                    //No further check required if matchs
                    return ParamArrayMatchs(args, i, dest.GetElementType(), conversions);
                }
                // matches current index
                if (i >= length)
                {
                    // method has one more parameter so skip
                    return conversions.Recycle();
                }

                var arg = args[i];
                if (arg is null)
                {
                    if (dest.IsValueType && !IsNullableType(dest))
                        return conversions.Recycle();
                }
                else
                {
                    var src = arg.GetType();
                    if (!TypeUtils.AreReferenceAssignable(dest, src))
                    {
                        if (TryImplicitConvert(src, dest, out MethodInfo opImplict) == false)
                            return conversions.Recycle();
                        conversions.Add(new ParamConversion(i, opImplict));
                    }
                }
            }
            if (i == length)
                return true;
            return conversions.Recycle();
        }

        static bool ParamArrayMatchs(object[] args, int index, Type dest, ArgumentConversions conversions)
        {
            var argConversions = new ArgumentConversions(args.Length - index);
            // check first parameter type matches
            for (int i = 0, current = index; current < args.Length; i++, current++)
            {
                var arg = args[current];
                if (arg is null)
                {
                    if (dest.IsValueType && !IsNullableType(dest))
                        return false;
                }
                else
                {
                    var src = arg.GetType();
                    if (!TypeUtils.AreReferenceAssignable(dest, src))
                    {
                        if (TryImplicitConvert(src, dest, out MethodInfo opImplict) == false)
                            return false;
                        argConversions.Add(new ParamConversion(i, opImplict));
                    }
                }
            }
            conversions.Add(new ParamArrayConversion(index, dest, argConversions));
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
        public static bool MatchesArgumentTypes(this MethodInfo m, params Type[] types)
        {
            if (m is null || types is null)
            {
                return false;
            }
            var ps = m.GetParameters();

            if (ps.Length != types.Length)
            {
                return false;
            }

            for (int i = 0; i < ps.Length; i++)
            {
                if (!TypeUtils.AreReferenceAssignable(ps[i].ParameterType, types[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool IsNullAssignable(this Type type)
        {
            return type.IsValueType == false || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(System.Nullable<>));
        }

        public static bool IsNullableType(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public static bool ImplementsGenericDefinition(this Type type, Type genericInterfaceDefinition, out Type implementingType)
        {
            if (!genericInterfaceDefinition.IsInterface || !genericInterfaceDefinition.IsGenericType)
            {
                throw new ArgumentNullException(string.Format("'{0}' is not a generic interface definition.", genericInterfaceDefinition));
            }

            if (type.IsInterface)
            {
                if (type.IsGenericType)
                {
                    Type interfaceDefinition = type.GetGenericTypeDefinition();

                    if (genericInterfaceDefinition == interfaceDefinition)
                    {
                        implementingType = type;
                        return true;
                    }
                }
            }

            foreach (Type i in type.GetInterfaces())
            {
                if (i.IsGenericType)
                {
                    Type interfaceDefinition = i.GetGenericTypeDefinition();

                    if (genericInterfaceDefinition == interfaceDefinition)
                    {
                        implementingType = i;
                        return true;
                    }
                }
            }

            implementingType = null;
            return false;
        }

        public static bool ImplementInterface(this Type self, Type ifaceType)
        {
            for(; ; )
            {
                if (self is null)
                    return false;
                Type[] interfaces = self.GetInterfaces();
                if(interfaces != null)
                {
                    for (int i = 0; i < interfaces.Length; i++)
                    {
                        if (interfaces[i] == ifaceType)
                            return true;
                        if (interfaces[i].ImplementInterface(ifaceType))
                            return true;
                    }
                }
                self = self.BaseType;
            }
        }

        public static bool TryImplicitConvert(this Type src, Type dest, out MethodInfo method)
        {
            // todo base class convert check
            var methods = (MethodInfo[])src.GetMember(TypeUtils.ImplicitConversionName, MemberTypes.Method, TypeUtils.PublicStatic);
            for (int i = 0; i < methods.Length; i++)
            {
                MethodInfo m = methods[i];
                if (MatchesArgumentTypes(m, src) && TypeUtils.AreReferenceAssignable(m.ReturnType, dest))
                {
                    method = m;
                    return true;
                }
            }
            methods = (MethodInfo[])dest.GetMember(TypeUtils.ImplicitConversionName, MemberTypes.Method, TypeUtils.PublicStatic);
            for (int i = 0; i < methods.Length; i++)
            {
                MethodInfo m = methods[i];
                if (MatchesArgumentTypes(m, src) && TypeUtils.AreReferenceAssignable(m.ReturnType, dest))
                {
                    method = m;
                    return true;
                }
            }
            method = null;
            return false;
        }

        public static bool TryExplicitConvert(this Type src, Type dest, out MethodInfo method)
        {
            // todo base class convert check
            var methods = (MethodInfo[])src.GetMember(TypeUtils.ExplicitConviersionName, MemberTypes.Method, TypeUtils.PublicStatic);
            for (int i = 0; i < methods.Length; i++)
            {
                MethodInfo m = methods[i];
                if (MatchesArgumentTypes(m, src) && TypeUtils.AreReferenceAssignable(m.ReturnType, dest))
                {
                    method = m;
                    return true;
                }
            }
            methods = (MethodInfo[])dest.GetMember(TypeUtils.ExplicitConviersionName, MemberTypes.Method, TypeUtils.PublicStatic);
            for (int i = 0; i < methods.Length; i++)
            {
                MethodInfo m = methods[i];
                if (MatchesArgumentTypes(m, src) && TypeUtils.AreReferenceAssignable(m.ReturnType, dest))
                {
                    method = m;
                    return true;
                }
            }
            method = null;
            return false;
        }

        #region Find Method
        public static bool TryFindMethod(this Type type, string name, object[] args, out MethodInfo method, out ArgumentConversions conversions)
        {
            conversions = new ArgumentConversions(args.Length);
            return type.IsInterface
                ? TryFindInterfaceMethod(type, name, args, out method, conversions)
                : type.IsDefined(typeof(RegisterAttribute), false)
                ? FindMethods(type, name, TypeUtils.AnyPublic, args, out method, conversions)
                : TryFindSystemMethod(name, type, TypeUtils.AnyPublic, args, out method, conversions);
        }

        public static bool FindMethods(this Type type, string name, BindingFlags flags, object[] args, out MethodInfo method, ArgumentConversions conversions)
        {
            if (type != null)
            {
                var methods = type.GetMethods(flags);
                for (int i = 0; i < methods.Length; i++)
                {
                    var m = methods[i];
                    var attrs = (RegisterAttribute[])m.GetCustomAttributes(typeof(RegisterAttribute), false);
                    if (attrs.Length > 0 && attrs[0].Match(name)
                        && MatchesArguments(m, args, conversions))
                    {
                        method = m;
                        return true;
                    }
                }
                return FindMethods(type.BaseType, name, TypeUtils.PublicStatic, args, out method, conversions);
            }
            method = null;
            return false;
        }

        static bool TryFindInterfaceMethod(this Type type, string name, object[] args, out MethodInfo method, ArgumentConversions conversions)
        {
            if (TryFindSystemMethod(name, type, TypeUtils.PublicInstance, args, out method, conversions))
                return true;
            var types = type.GetInterfaces();
            for (int i = 0; i < types.Length; i++)
            {
                type = types[i];
                if (TryFindSystemMethod(name, type, TypeUtils.PublicInstance, args, out method, conversions))
                    return true;
            }
            return false;
        }

        private static bool TryFindSystemMethod(string name, Type type, BindingFlags flags, object[] args, out MethodInfo method, ArgumentConversions conversions)
        {
            if (type != null)
            {
                foreach (MethodInfo m in type.GetMethods(flags))
                {
                    if (m.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && MatchesArguments(m, args, conversions))
                    {
                        method = m;
                        return true;
                    }
                }
            }

            method = null;
            return false;
        }
        #endregion

        #region Indexer
        /// Current Declared Indexer can get
        public static MethodInfo FindGetIndexer(this Type type, object[] args, out ArgumentConversions conversions)
        {
            conversions = new ArgumentConversions(args.Length);
            if (type.IsArray)
            {
                var m = type.GetMethod("Get", TypeUtils.PublicInstance);
                //for array no indexer we have to use Get method
                if (MatchesArguments(m, args, conversions))
                {
                    return m;
                }
            }
            foreach (var item in type.GetDefaultMembers())
            {
                if (item.MemberType == MemberTypes.Property)
                {
                    var p = (PropertyInfo)item;
                    if (p.CanRead)
                    {
                        var m = p.GetGetMethod(true);
                        if (MatchesArguments(m, args, conversions))
                        {
                            return m;
                        }
                    }
                }
            }
            return null;
        }

        /// Current Declared Indexer can get
        public static MethodInfo FindSetIndexer(this Type type, object[] indices, object value, out ArgumentConversions conversions, out object[] args)
        {
            int length = indices.Length;
            args = new object[length + 1];
            indices.CopyTo(args, 0);
            args[length] = value;
            return FindSetIndexer(type, args, out conversions);
        }

        public static MethodInfo FindSetIndexer(this Type type, object[] args, out ArgumentConversions conversions)
        {
            conversions = new ArgumentConversions(args.Length);
            if (type.IsArray)
            {
                var m = type.GetMethod("Set", TypeUtils.PublicInstance);
                //for array no indexer we have to use Set Method
                if (MatchesArguments(m, args, conversions))
                {
                    return m;
                }
                return null;
            }
            foreach (var item in type.GetDefaultMembers())
            {
                if (item.MemberType == MemberTypes.Property)
                {
                    var p = (PropertyInfo)item;
                    if (p.CanWrite)
                    {
                        var m = p.GetSetMethod(true);
                        if (MatchesArguments(m, args, conversions))
                        {
                            return m;
                        }
                    }
                }
            }
            return null;
        }
        #endregion

        #region Member
        public static bool TryFindMember(this Type type, string name, BindingFlags flags, out IMemberBinder binder)
        {
            if (type.IsDefined(typeof(RegisterAttribute), false))
            {
                return FindMember(type, name, flags, out binder);
            }
            binder = FindSystemMember(type, name, flags);
            return binder != null;
        }

        public static bool FindMember(this Type type, string name, BindingFlags flags, out IMemberBinder binder)
        {
            if (type != null)
            {
                var properties = type.GetProperties(flags);
                for (int i = 0; i < properties.Length; i++)
                {
                    var p = properties[i];
                    var data = (Attribute[])p.GetCustomAttributes(typeof(RegisterAttribute), false);
                    if (data.Length > 0 && data[0].Match(name))
                    {
                        binder = new PropertyBinder(p);
                        return true;
                    }
                }

                var fields = type.GetFields(flags);
                for (int i = 0; i < fields.Length; i++)
                {
                    var f = fields[i];
                    var data = (System.Attribute[])f.GetCustomAttributes(typeof(RegisterAttribute), false);
                    if (data.Length > 0 && data[0].Match(name))
                    {
                        binder = new FieldBinder(f);
                        return true;
                    }
                }
                return FindMember(type.BaseType, name, flags, out binder);
            }
            binder = null;
            return false;
        }

        public static IMemberBinder FindSystemMember(this Type type, string name, BindingFlags flags)
        {
            if (type != null)
            {
                var p = type.GetProperty(name, flags | BindingFlags.IgnoreCase);
                if (p != null)
                    return new PropertyBinder(p);
                var f = type.GetField(name, flags);
                if (f != null)
                    return new FieldBinder(f);
                return FindSystemMember(type.BaseType, name, flags);
            }
            return null;
        }
        #endregion

        public static object InvokeDelegate(this Delegate self, object[] args)
        {
            var conversions = new ArgumentConversions(args.Length);
            var method = self.GetType().GetMethod(nameof(Action.Invoke));
            if (method.MatchesArguments(args, conversions))
            {
                if (conversions.Count > 0)
                    conversions.Invoke(ref args);
                return Any.op_Implicit(self.DynamicInvoke(args));
            }
            return Any.Empty;
        }
    }
}
