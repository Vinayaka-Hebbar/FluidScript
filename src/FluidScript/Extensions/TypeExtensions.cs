using FluidScript.Compiler;
using FluidScript.Utils;
using System.Reflection;

namespace FluidScript.Extensions
{
    public static class TypeExtensions
    {
        private const string Separator = ", ";
        private const BindingFlags DeclaredStatic = DeclaredPublic | BindingFlags.Static;
        private const BindingFlags DeclaredInstance = DeclaredPublic | BindingFlags.Instance;
        private const BindingFlags DeclaredPublic = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly | BindingFlags.ExactBinding;
        public static ConstructorInfo GetInstanceCtor(this System.Type type, params System.Type[] parameterTypes)
        {
            var result = type.GetConstructor(DeclaredInstance, null, parameterTypes, null);
            if (result == null)
                throw new System.InvalidOperationException(string.Format("the ctor {0}.ctor({1})", type.FullName, StringHelpers.Join(Separator, parameterTypes)));
            return result;
        }

        public static MethodInfo GetStaticMethod(this System.Type type, string name, params System.Type[] parameterTypes)
        {
            MethodInfo result = type.GetMethod(name, DeclaredStatic, null, parameterTypes, null);
            if (result == null)
                throw new System.InvalidOperationException(string.Format("the static method {0}.{1}({2})", type.FullName, name, StringHelpers.Join(Separator, parameterTypes)));
            return result;
        }

        public static MethodInfo GetImplicitConversion(this System.Type type, string name, System.Type returnType, params System.Type[] parameterTypes)
        {
            var results = type.GetMember(name, MemberTypes.Method, DeclaredStatic);
            foreach (MethodInfo method in results)
            {
                if (TypeUtils.MatchesArgumentTypes(method, parameterTypes) && TypeUtils.AreReferenceAssignable(method.ReturnType, returnType))
                {
                    return method;
                }
            }
            throw new System.InvalidOperationException(string.Format("the convertion method {0}.{1}({2})", type.FullName, name, StringHelpers.Join(Separator, parameterTypes)));
        }

        public static FieldInfo GetField(this System.Type type, string name, BindingFlags binding)
        {
            FieldInfo result = type.GetField(name, binding);
            if (result == null)
                throw new System.InvalidOperationException(string.Format("the field {0}.{1}", type.FullName, name));
            return result;

        }

        public static MethodInfo GetInstanceMethod(this System.Type type, string name, params System.Type[] parameterTypes)
        {
            MethodInfo result = type.GetMethod(name, DeclaredInstance, null, parameterTypes, null);
            if (result == null)
                throw new System.InvalidOperationException(string.Format("The instance method {0}.{1}({2}) does not exist.", type.FullName, name, StringHelpers.Join(", ", parameterTypes)));
            return result;
        }
    }
}
