using FluidScript.Compiler.Metadata;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FluidScript.Core
{
#if Runtime
    public class ObjectInstance : RuntimeObject, IEnumerable<KeyValuePair<object,RuntimeObject>>
    {
        protected IDictionary<object, RuntimeObject> values;

        private readonly Prototype prototype;

        public ObjectInstance(Prototype prototype)
        {
            this.prototype = prototype;
            values = prototype.Init(this);
        }

        protected ObjectInstance()
        {
            this.prototype = GetPrototype();
            values = prototype.Init(this);
        }

        public override RuntimeObject this[string name]
        {
            get
            {
                if (values.ContainsKey(name))
                    return values[name];
                return Undefined;
            }
            set
            {
                if (value.ReflectedType == RuntimeType.Function)
                {
                    AttachFunction(name, value);
                    return;
                }
                values[name] = value;
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

        public override bool ContainsKey(object key)
        {
            return values != null ? values.ContainsKey(key) : false;
        }

        public override RuntimeObject Call(string name, params RuntimeObject[] args)
        {
            if (values.ContainsKey(name))
            {
                return values[name].DynamicInvoke(args);
            }
            return base.Call(name, args);
        }

        [Compiler.Reflection.Callable("print", RuntimeType.Void, Compiler.Emit.ArgumentTypes.Any)]
        internal RuntimeObject Print(RuntimeObject value)
        {
            System.Console.WriteLine(value);
            return null;
        }

        [Compiler.Reflection.Callable("print", RuntimeType.Void, Compiler.Emit.ArgumentTypes.Any, Compiler.Emit.ArgumentTypes.VarArg)]
        internal RuntimeObject Print(RuntimeObject obj, RuntimeObject[] args)
        {
            System.Console.WriteLine(string.Format(obj.ToString(), args.Select(value => value.ToString()).ToArray()));
            return null;
        }

        public override RuntimeObject GetConstantValue(string name)
        {
            return prototype.GetConstant(name);
        }

        public override string ToString()
        {
            return string.Concat("\n{", string.Join(",", values.Select(item => string.Concat(item.Key, ":", item.Value.ToString()))), "}");
        }

        public override Prototype GetPrototype()
        {
            return prototype;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return values.GetEnumerator();
        }

        IEnumerator<KeyValuePair<object, RuntimeObject>> IEnumerable<KeyValuePair<object, RuntimeObject>>.GetEnumerator()
        {
            return values.GetEnumerator();
        }
    }
#endif
}
