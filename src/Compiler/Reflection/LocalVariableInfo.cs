using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluidScript.Compiler.Reflection
{
    public class LocalVariableInfo
    {
        public readonly string Name;
        public readonly TypeInfo Type;
        public readonly int Index;

        public LocalVariableInfo(string name, TypeInfo type, int index)
        {
            Name = name;
            Type = type;
            Index = index;
        }
    }
}
