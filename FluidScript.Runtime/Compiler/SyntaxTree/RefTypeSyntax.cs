using System;
using FluidScript.Reflection;

namespace FluidScript.Compiler.SyntaxTree
{
    public class RefTypeSyntax : TypeSyntax
    {
        public readonly string Name;

        public RefTypeSyntax(string name)
        {
            Name = name;
        }

        public override ITypeInfo GetTypeInfo()
        {
            return new TypeInfo(Name);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
