using FluidScript.Core;
using System;
using System.Reflection;

namespace FluidScript.Compiler.Emit
{
    internal static class ReflectionHelpers
    {
        private const string Separator = ", ";
        #region Object
        internal static MethodInfo ObjectEquals_Two_Object;
        internal static MethodInfo Object_ToString;
        internal static MethodInfo StringConcat_Two_String;
        internal static MethodInfo StringConcat_Two_Object;
        #endregion

        static ReflectionHelpers()
        {
            ObjectEquals_Two_Object = GetInstanceMethod(typeof(object), "Equals", typeof(object), typeof(object));
            Object_ToString = GetInstanceMethod(typeof(object), "ToString");
            StringConcat_Two_String = GetStaticMethod(typeof(string), "Concat", typeof(string), typeof(string));
            StringConcat_Two_Object = GetStaticMethod(typeof(string), "Concat", typeof(object), typeof(object));
        }

        public static MethodInfo GetStaticMethod(Type type, string name, params Type[] parameterTypes)
        {
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.ExactBinding;
            MethodInfo result = type.GetMethod(name, flags, null, parameterTypes, null);
            if (result == null)
                throw new InvalidOperationException(string.Format("the static method {0}.{1}({2})", type.FullName, name, StringHelpers.Join(Separator, parameterTypes)));
            return result;
        }

        public static MethodInfo GetInstanceMethod(Type type, string name, params Type[] parameterTypes)
        {
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.ExactBinding;
            MethodInfo result = type.GetMethod(name, flags, null, parameterTypes, null);
            if (result == null)
                throw new InvalidOperationException(string.Format("The instance method {0}.{1}({2}) does not exist.", type.FullName, name, StringHelpers.Join<Type>(", ", parameterTypes)));
            return result;
        }
    }
}
