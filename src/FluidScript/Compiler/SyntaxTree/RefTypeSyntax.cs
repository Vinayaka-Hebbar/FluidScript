using FluidScript.Reflection;
using FluidScript.Reflection.Emit;
using System;

namespace FluidScript.Compiler.SyntaxTree
{
    public class RefTypeSyntax : TypeSyntax
    {
        public readonly string Name;

        public RefTypeSyntax(string name)
        {
            Name = name;
        }

        public override Type GetType(ITypeProvider provider)
        {
            return provider.GetType(Name);
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
