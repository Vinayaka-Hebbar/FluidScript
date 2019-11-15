using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FluidScript.Core
{
#if Runtime
    public sealed class ArrayObject : ObjectInstance, IEnumerable<RuntimeObject>
    {
        private static Compiler.Metadata.Prototype prototype;
        private RuntimeObject[] store;
        private readonly RuntimeType type;

        public ArrayObject(RuntimeObject[] store, RuntimeType type) 
        {
            this.store = store;
            this.type = type;
        }

        public RuntimeObject this[int index]
        {
            get
            {
                return store[index];
            }
            set
            {
                if ((store.Length > index) == false)
                {
                    System.Array.Resize(ref store, index + 1);
                }
                store[index] = value;
            }
        }

        public override RuntimeType ReflectedType => RuntimeType.Array | type;

        public int Length => store.Length;

        [Compiler.Reflection.Callable("length")]
        internal RuntimeObject Size()
        {
            return new PrimitiveObject(store.Length);
        }

        public void Resize(int newSize)
        {
            System.Array.Resize(ref store, newSize);
        }

        [Compiler.Reflection.Callable("indexOf", RuntimeType.Int32, Compiler.Emit.ArgumentTypes.Double)]
        public RuntimeObject IndexOf(RuntimeObject arg1)
        {
            return System.Array.IndexOf(store, arg1);
        }

        [Compiler.Reflection.Callable("setItem", RuntimeType.Void, Compiler.Emit.ArgumentTypes.Int32, Compiler.Emit.ArgumentTypes.Any)]
        public void SetItem(RuntimeObject index, RuntimeObject value)
        {
            var i = index.ToInt32();
            if (store.Length <= i)
            {
                System.Array.Resize(ref store, i + 1);
            }
            store[i] = value;
        }

        public override string ToString()
        {
            return string.Concat("[", string.Join(",", store.Select(value => value.ToString())), "]");
        }

        public override RuntimeObject DynamicInvoke(RuntimeObject[] args)
        {
            var arg = args[0];
            int index = arg.ToInt32();
            if (index >= store.Length)
                return Undefined;
            var value = store[index];
            if ((value.ReflectedType & RuntimeType.Array) == RuntimeType.Array)
            {
                return value.DynamicInvoke(args.Skip(1).ToArray());
            }
            return value;
        }

        IEnumerator<RuntimeObject> IEnumerable<RuntimeObject>.GetEnumerator()
        {
            return ((IEnumerable<RuntimeObject>)store).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return store.GetEnumerator();
        }

        public override Compiler.Metadata.Prototype GetPrototype()
        {
            if (prototype == null)
                prototype = Compiler.Metadata.Prototype.Create(GetType());
            return prototype;
        }
    }
#endif
}
