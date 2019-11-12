using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FluidScript.Core
{
    public sealed class ArrayObject : RuntimeObject, IEnumerable<RuntimeObject>
    {
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

        public override RuntimeType RuntimeType => type;

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

        public override bool IsArray()
        {
            return true;
        }

        public override bool IsBool()
        {
            return false;
        }

        public override bool IsChar()
        {
            return false;
        }

        public override bool IsNull()
        {
            return store == null;
        }

        public override bool IsNumber()
        {
            return false;
        }

        public override bool IsString()
        {
            return true;
        }

        public override bool ToBool()
        {
            return false;
        }

        public override char ToChar()
        {
            return char.MinValue;
        }

        public override double ToDouble()
        {
            return double.NaN;
        }

        public override float ToFloat()
        {
            return float.NaN;
        }

        public override int ToInt32()
        {
            return int.MinValue;
        }

        public override double ToNumber()
        {
            return double.NaN;
        }

        [Compiler.Reflection.Callable("indexOf", Compiler.Emit.ArgumentTypes.Double)]
        public RuntimeObject IndexOf(RuntimeObject arg1)
        {
            return System.Array.IndexOf(store, arg1);
        }

        public override string ToString()
        {
            return string.Concat("[", string.Join(",", store.Select(value => value.ToString())), "]");
        }

        public IEnumerator<RuntimeObject> GetEnumerator()
        {
            return ((IEnumerable<RuntimeObject>)store).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return store.GetEnumerator();
        }
    }
}
