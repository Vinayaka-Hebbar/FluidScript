using FluidScript.Compiler.Metadata;
using System.Collections.Generic;
using System.Linq;

namespace FluidScript.Core
{
#if Runtime
    public class LocalInstance : RuntimeObject
    {
        private readonly Prototype prototype;
        private readonly RuntimeObject top;

        protected readonly IDictionary<object, RuntimeObject> values;
        public LocalInstance(Prototype prototype, RuntimeObject obj)
        {
            this.prototype = prototype;
            top = obj;
            values = prototype.Init(this);
        }

        public override RuntimeObject this[string name]
        {
            get
            {
                if (values.ContainsKey(name))
                    return values[name];
                if (top is object)
                    return top[name];
                return Undefined;
            }
            set
            {

                if (values.ContainsKey(name))
                {
                    if (value.ReflectedType == RuntimeType.Function)
                    {
                        AttachFunction(name, value);
                        return;
                    }
                    values[name] = value;
                }
                if (top is object)
                    top[name] = value;
            }
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
            return top.Call(name, args); ;
        }

        public override RuntimeObject GetConstantValue(string name)
        {
            return prototype.GetConstant(name);
        }

        public override string ToString()
        {
            return string.Concat("{", string.Join(",\n\r", values.Select(item => string.Concat(item.Key, ":", item.Value.ToString()))), "}");
        }

        public override Prototype GetPrototype()
        {
            return prototype;
        }
    }
#endif
}
