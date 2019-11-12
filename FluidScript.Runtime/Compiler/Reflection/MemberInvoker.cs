using System.Linq;
using System.Reflection;

namespace FluidScript.Compiler.Reflection
{
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
    }
}
