using System;
using System.Collections.Generic;
using System.Text;

namespace FluidScript.Math.Compiler.Reflection
{
    public class DeclaredMethod
    {
        public readonly string Name;
        public readonly Type[] ArgumentTypes;
        public System.Reflection.MethodInfo Store;
    }
}
