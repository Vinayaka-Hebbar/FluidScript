using FluidScript.Compiler.Metadata;
using System.Linq;

namespace FluidScript.Library
{
#if Runtime
    internal sealed class LocalInstance : RuntimeObject
    {
        private readonly RuntimeObject top;
        public LocalInstance(Prototype prototype, RuntimeObject obj) : base(prototype)
        {
            top = obj;
        }

        public override RuntimeObject this[object name]
        {
            get
            {
                if (instances.ContainsKey(name))
                    return instances[name];
                if (top is object)
                    return top[name];
                return Undefined;
            }
            set
            {

                if (instances.ContainsKey(name))
                {
                    base[name] = value;
                    return;
                }
                if (top is object)
                    top[name] = value;
            }
        }

        public override RuntimeObject Call(string name, params RuntimeObject[] args)
        {
            if (instances.ContainsKey(name))
            {
                return instances[name].DynamicInvoke(args);
            }
            return top.Call(name, args); ;
        }
    }
#endif
}
