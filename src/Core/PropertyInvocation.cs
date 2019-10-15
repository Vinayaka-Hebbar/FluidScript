using System.Linq;
using System.Reflection;

namespace FluidScript.Core
{
    public struct PropertyInvocation : IInvocationContext
    {
        public readonly string Name;
        public readonly IInvocationContext Context;
        public readonly Expression.Operation OpCode;
        public readonly Expression.Operation ParentKind;

        public PropertyInvocation(string name, IInvocationContext context, Expression.Operation opCode, Expression.Operation parentKind)
        {
            Name = name;
            Context = context;
            OpCode = opCode;
            ParentKind = parentKind;
        }

        public bool CanInvoke => true;

        public Object Invoke(Object args)
        {
            if (ParentKind == Expression.Operation.Invocation)
            {
                var argValues = args.ToArray();
                if (Context.CanInvoke && Context is IMethodInvocation invocation)
                {
                    var count = argValues.Length;
                    var obj = invocation.Invoke(Name, OpCode, null, argValues);
#if NET35 || NET40 || NET45
                    var type = obj.ToType();
                    var declaredMethod = type.GetMethod(Name, argValues.Select(arg => arg.GetType()).ToArray());
#else
                    var type = obj.ToType().GetTypeInfo();
                    var declaredMethod = type.GetDeclaredMethods(Name).FirstOrDefault(method => method.GetParameters().Length == count);
#endif
                    if (declaredMethod == null)
                        throw new System.MethodAccessException($"Method {Name} not found in {type.FullName}");
                    var result = declaredMethod.Invoke(obj.Raw, argValues);
                    return new Object(result);
                }
                return Object.Null;
            }
            else
            {
                System.Type type = typeof(object);
                Object value = Object.Null;
                if (Context.CanInvoke && Context is IPropertyInvocation invocation)
                {
                    value = invocation.Invoke(Name, OpCode, value);
                    type = value.ToType();
                }
                if (Context is TypeNameContext typeName)
                {
                    type = System.Type.GetType(typeName.Name);
                }
#if NET35 || NET40 || NET45
                var property = type.GetProperty(Name);
#else
                 var property = type.GetTypeInfo().GetDeclaredProperty(Name);
#endif

                var result = property.GetValue(value.Raw, new object[0]);
                return new Object(result);
            }
        }
    }
}
