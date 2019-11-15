using FluidScript.Compiler.Metadata;
using System.Collections.Generic;
using System.Linq;

namespace FluidScript.Core
{
#if Runtime
    public class FunctionInstance : RuntimeObject
    {
        private readonly Prototype prototype;
        protected readonly IDictionary<object, RuntimeObject> values;
        public FunctionInstance(Prototype prototype, RuntimeObject obj)
        {
            this.prototype = prototype;
            values = prototype.Init(this, new KeyValuePair<object, RuntimeObject>("this", obj));
        }

        public override RuntimeObject this[string name]
        {
            get
            {
                if (values.ContainsKey(name))
                    return values[name];
                return values["this"][name];
            }
            set
            {
                if (values.ContainsKey(name))
                {
                    Attach(name, value);
                    return;
                }
                RuntimeObject top = values["this"];
                if (top.ContainsKey(name))
                    top[name] = value;
                Attach(name, value);

            }
        }

        private void Attach(string name, RuntimeObject value)
        {
            if (value.ReflectedType == RuntimeType.Function)
            {
                AttachFunction(name, value);
                return;
            }
            values[name] = value;
            return;
        }

        private void AttachFunction(string name, RuntimeObject value)
        {
            FunctionGroup list = null;
            if (values.TryGetValue(name, out RuntimeObject runtime))
            {
                if (runtime is FunctionGroup)
                {
                    list = (FunctionGroup)value;
                }
            }
            if (list is null)
            {
                list = new FunctionGroup(name);
                values.Add(name, list);
            }
            list.Add((IFunctionReference)value);
        }

        public override RuntimeObject Call(string name, params RuntimeObject[] args)
        {
            if (values.ContainsKey(name))
            {
                return values[name].DynamicInvoke(args);
            }
            return values["this"].Call(name, args); ;
        }

        public override RuntimeObject GetConstantValue(string name)
        {
            return prototype.GetConstant(name);
        }

        public override string ToString()
        {
            return string.Concat("\n{", string.Join(",\n", values.Skip(1).Select(item => string.Concat(item.Key, ":", item.Value.ToString()))), "}");
        }

        public override Prototype GetPrototype()
        {
            return prototype;
        }
    }
#endif
}
