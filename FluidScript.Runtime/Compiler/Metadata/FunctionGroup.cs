using System;
using System.Collections.Generic;
using System.Linq;

namespace FluidScript.Compiler.Metadata
{
#if Runtime
    public sealed class FunctionGroup : RuntimeObject
    {
        private static Prototype prototype;
        public readonly string Name;
        private IFunctionReference[] References;

        public FunctionGroup(string name) 
        {
            Name = name;
            References = new IFunctionReference[0];
        }

        public void Add(IFunctionReference reference)
        {
            int index = References.Length;
            Array.Resize(ref References, index + 1);
            References[index] = reference;
        }

        public override RuntimeObject DynamicInvoke(RuntimeObject[] args)
        {
            var types = FilterTypes(args).ToArray();
            var method = References.FirstOrDefault(m => Emit.TypeUtils.TypesEqual(m.Arguments, types));
            if (method is object)
            {
                return method.DynamicInvoke(args);
            }
            return Null;
        }

        private static IEnumerable<RuntimeType> FilterTypes(System.Collections.Generic.IEnumerable<RuntimeObject> args)
        {
            foreach (var arg in args)
            {
                yield return arg is null ? RuntimeType.Undefined : arg.ReflectedType;
            }
        }

        public override string ToString()
        {
            return string.Join(",", References.Select(reference => reference.ToString()));
        }

        public override Prototype GetPrototype()
        {
            if (prototype is null)
            {
                var baseProto = new DefaultObjectPrototype(new Reflection.DeclaredMethod[0]);
                var methods = Reflection.TypeHelper.GetMethods(GetType());
                prototype = new ObjectPrototype(methods, null, baseProto, "FunctionGroup")
                {
                    IsSealed = true
                };
            }
            return prototype;
        }
    }
#endif
}
