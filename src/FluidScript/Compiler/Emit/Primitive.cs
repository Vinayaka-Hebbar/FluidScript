﻿using System;

namespace FluidScript.Compiler.Emit
{
    public
#if LATEST_VS
        readonly
#endif
        struct Primitive
    {
        public readonly string Name;
        public readonly Type Type;

        public Primitive(string name, Type type)
        {
            Name = name;
            Type = type;
        }

        public override string ToString()
        {
            return Type.FullName;
        }
    }
}
