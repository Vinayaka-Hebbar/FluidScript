using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FluidScript.Compiler.Reflection
{
#if Runtime
    public static class MemberInvoker
    {
        internal const BindingFlags Any = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;

        public static RuntimeObject Invoke(object instance, string name, RuntimeObject[] args)
        {
            var type = instance.GetType();
            var types = args.Select(arg => arg.DeclaredType).ToArray();
            var methods = type.GetMethods(Any)
                .Where(m => m.IsDefined(typeof(Callable), false));
            var method = methods.FirstOrDefault(m =>
            {
                var attribute = m.GetCustomAttributes(typeof(Callable), false).First();
                return ((Callable)attribute).Name == name;
            });
            if (method == null)
                return RuntimeObject.Undefined;
            var value = (RuntimeObject)method.Invoke(instance, args);
            return value;
        }

        public static RuntimeObject Invoke(object instance, string name)
        {
            var type = instance.GetType();
            var methods = type.GetMembers(Any)
                .Where(m => m.IsDefined(typeof(Callable), false));
            var member = methods.FirstOrDefault(m =>
            {
                var attribute = m.GetCustomAttributes(typeof(Callable), false).First();
                return ((Callable)attribute).Name == name;
            });
            if (member == null)
                return RuntimeObject.Undefined;
            switch (member.MemberType)
            {
                case MemberTypes.Property:
                    var property = (PropertyInfo)member;
                    return (RuntimeObject)property.GetValue(instance, new object[0]);
                case MemberTypes.Method:
                    var method = (MethodInfo)member;
                    return (RuntimeObject)method.Invoke(instance, new object[0]);
            }
            return RuntimeObject.Null;
        }

        public static IEnumerable<DeclaredMethod> GetMethods(Type type)
        {
            int index = 0;
            var methods = type.GetMethods(Any)
                 .Where(m => m.IsDefined(typeof(Callable), false));
            foreach (MethodInfo method in methods)
            {
                var attribute = (Callable)method.GetCustomAttributes(typeof(Callable), false).First();
                var parameters = method.GetParameters();
                var argTypes = attribute.Arguments;
                var arguments = new Emit.ArgumentType[parameters.Length];
                for (int i = 0; i < parameters.Length; i++)
                {
                    var arg = argTypes[i];
                    var flags = DeclaredFlags.None;
                    if ((arg & Emit.ArgumentTypes.VarArg) == Emit.ArgumentTypes.VarArg)
                    {
                        arg ^= Emit.ArgumentTypes.VarArg;
                        flags |= DeclaredFlags.VarArgs;
                    }
                    arguments[i] = new Emit.ArgumentType(parameters[i].Name, (RuntimeType)arg, flags);
                }
                yield return new DeclaredMethod(attribute.Name, index++, arguments, attribute.ReturnType) { Store = method };
            }
        }
    }
#endif
}
