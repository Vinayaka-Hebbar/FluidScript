using System.Collections.Generic;
using System.Linq;

namespace FluidScript.Library
{
    public class DictionaryObject : RuntimeObject, IEnumerable<RuntimeObject>
    {
        private static Compiler.Metadata.Prototype prototype;
        private IDictionary<RuntimeObject, RuntimeObject> store;
        private readonly RuntimeType type;

        public DictionaryObject(IDictionary<RuntimeObject, RuntimeObject> store, RuntimeType type)
        {
            this.store = store;
            this.type = type;
        }

        public override RuntimeType ReflectedType => RuntimeType.Array | type;

        public int Length => store.Count;

        [Compiler.Reflection.Property("length", RuntimeType.Int32)]
        internal RuntimeObject Size
        {
            get
            {
                return new PrimitiveObject(store.Count);
            }
        }

        [Compiler.Reflection.Callable("hasKey", RuntimeType.Bool, Compiler.Emit.ArgumentTypes.Any)]
        public RuntimeObject HasKey(RuntimeObject arg1)
        {
            return store.ContainsKey(arg1);
        }

        [Compiler.Reflection.Callable("setItem", RuntimeType.Void, Compiler.Emit.ArgumentTypes.Int32, Compiler.Emit.ArgumentTypes.Any)]
        public void SetItem(RuntimeObject index, RuntimeObject value)
        {
            if (store.ContainsKey(index))
                store[index] = value;
        }

        public override string ToString()
        {
            return string.Concat("[", string.Join(",", store.Select(value => value.ToString())), "]");
        }

        public override RuntimeObject DynamicInvoke(RuntimeObject[] args)
        {
            var arg = args[0];
            if (store.ContainsKey(arg) == false)
                return Undefined;
            return store[arg];
        }

        public override Compiler.Metadata.Prototype GetPrototype()
        {
            if (prototype is null)
            {
                prototype = Compiler.Metadata.Prototype.Create(GetType());
                prototype.IsSealed = true;
            }
            return prototype;
        }

        public IEnumerator<RuntimeObject> GetEnumerator()
        {
            return store.Keys.GetEnumerator();
        }
    }
}
