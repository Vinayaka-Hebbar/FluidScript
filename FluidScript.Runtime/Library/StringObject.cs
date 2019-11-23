using FluidScript.Compiler.Metadata;
using System.Collections;
using System.Collections.Generic;

namespace FluidScript.Library
{
    public sealed class StringObject : RuntimeObject, IEnumerable<char>
    {
        private static Prototype prototype;
        private readonly string store;
        public StringObject(string value)
        {
            store = value;
        }

        public override RuntimeType ReflectedType => RuntimeType.String;

        [Compiler.Reflection.Callable("elementAt", RuntimeType.Char, Compiler.Emit.ArgumentTypes.Int32)]
        internal RuntimeObject ElementAt(RuntimeObject index) => new PrimitiveObject(store[index.ToInt32()]);

        public override string ToString()
        {
            return store;
        }

        public override Prototype GetPrototype()
        {
            if (prototype is null)
            {
                prototype = Prototype.Create(GetType());
                prototype.IsSealed = true;
            }
            return prototype;
        }

        [Compiler.Reflection.Property("length", RuntimeType.Int32)]
        internal RuntimeObject Length
        {
            get => store.Length;
        }

        [Compiler.Reflection.Callable("reverse", RuntimeType.String)]
        internal RuntimeObject Reverse()
        {
            return new StringObject(new string(System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Reverse(store))));
        }

        [Compiler.Reflection.Callable("toUpper", RuntimeType.String)]
        internal RuntimeObject ToUpper()
        {
            return new StringObject(store.ToUpper());
        }

        [Compiler.Reflection.Callable("toLower", RuntimeType.String)]
        internal RuntimeObject ToLower()
        {
            return new StringObject(store.ToLower());
        }

        public override RuntimeObject DynamicInvoke(RuntimeObject[] args)
        {
            var arg = args[0];
            int index = arg.ToInt32();
            //todo empty char
            if (index >= store.Length)
                return Undefined;
            return store[index];
        }

        public IEnumerator<char> GetEnumerator()
        {
            return store.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return store.GetEnumerator();
        }
    }
}
