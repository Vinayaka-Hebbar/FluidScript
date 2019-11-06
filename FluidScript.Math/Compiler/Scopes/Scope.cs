using System;
using System.Collections.Generic;
using System.Text;

namespace FluidScript.Math.Compiler.Scopes
{
    public abstract class Scope
    {
        public readonly Scope Parent;
        protected Scope(Scope parent)
        {
            Parent = parent;
        }
    }
}
