using FluidScript.Compiler.Metadata;
using System.Linq;

namespace FluidScript.Core
{
#if Runtime
    internal sealed class FunctionInstance : RuntimeObject
    {
        public FunctionInstance(Prototype prototype, RuntimeObject obj) : base(prototype, obj)
        {
        }

        public override RuntimeObject this[object name]
        {
            get
            {
                if (instances.ContainsKey(name))
                    return instances[name];
                return instances["this"][name];
            }
            set
            {
                if (instances.ContainsKey(name))
                {
                    base[name] = value;
                    return;
                }
                RuntimeObject top = instances["this"];
                if (top.ContainsKey(name))
                {
                    top[name] = value;
                    return;
                }
                base[name] = value;

            }
        }

        public override RuntimeObject Call(string name, params RuntimeObject[] args)
        {
            if (instances.ContainsKey(name))
            {
                return instances[name].DynamicInvoke(args);
            }
            return instances["this"].Call(name, args); ;
        }

        public override string ToString()
        {
            return instances.ToStringLocal();
        }
    }
#endif
}
