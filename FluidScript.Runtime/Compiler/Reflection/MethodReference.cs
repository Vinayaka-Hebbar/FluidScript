using System;

namespace FluidScript.Runtime.Compiler.Reflection
{
    public class MethodReference
    {
        private readonly Delegate _onInvoke;

        public MethodReference(Delegate onInvoke)
        {
            _onInvoke = onInvoke;
        }
    }
}
