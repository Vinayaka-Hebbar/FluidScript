using System;
using System.Linq;

namespace FluidScript.Core
{
#if Runtime
    public sealed class FunctionGroup : RuntimeObject
    {
        private static Compiler.Metadata.Prototype prototype;
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
            var method = References.FirstOrDefault(m => Reflection.Emit.TypeUtils.TypesEqual(m.Arguments, types));
            if (method is object)
            {
                return method.DynamicInvoke(args);
            }
            return Null;
        }

        private static System.Collections.Generic.IEnumerable<RuntimeType> FilterTypes(System.Collections.Generic.IEnumerable<RuntimeObject> args)
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

        public override Compiler.Metadata.Prototype GetPrototype()
        {
            if (prototype is null)
            {
                var baseProto = new Compiler.Metadata.DefaultObjectPrototype(new Reflection.DeclaredMethod[0]);
                var methods = Reflection.TypeHelper.GetMethods(GetType());
                prototype = new Compiler.Metadata.ObjectPrototype(methods, null, baseProto, "FunctionGroup")
                {
                    IsSealed = true
                };
            }
            return prototype;
        }
    }
#endif
}
