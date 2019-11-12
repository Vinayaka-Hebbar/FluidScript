using System;

namespace FluidScript.Compiler.Reflection
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
    public class Callable : Attribute
    {
        public readonly string Name;

        public readonly Emit.ArgumentTypes[] Arguments;

        public Callable(string name)
        {
            Name = name;
        }

        public Callable(string name, params Emit.ArgumentTypes[] args)
        {
            Name = name;
            Arguments = args;
        }

        public Emit.ArgumentType[] GetArgumentTypes()
        {
            if (Arguments == null)
                return new Emit.ArgumentType[0];
            var args = new Emit.ArgumentType[Arguments.Length];
            for (int index = 0; index < Arguments.Length; index++)
            {
                Emit.ArgumentTypes arg = Arguments[index];
                var flags = Emit.ArgumentFlags.None;
                if ((arg & Emit.ArgumentTypes.VarArg) == Emit.ArgumentTypes.VarArg)
                {
                    arg ^= Emit.ArgumentTypes.VarArg;
                    flags |= Emit.ArgumentFlags.VarArg;
                }
                args[index] = new Emit.ArgumentType((RuntimeType)arg, flags);
            }
            return args;
        }
    }
}
