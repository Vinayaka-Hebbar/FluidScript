using FluidScript.Compiler.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluidScript.Compiler.Scopes
{
    public class ModuleScope : Scope
    {
        private readonly AssemblyInfo assembly;
        public ModuleScope(SyntaxVisitor visitor, AssemblyInfo assembly) : base(visitor)
        {
            this.assembly = assembly;
        }

        public override AssemblyInfo GetAssemblyInfo()
        {
            return assembly;
        }
    }
}
