using FluidScript.Compiler.Emit;
using System.Reflection.Emit;

namespace FluidScript.Compiler.SyntaxTree
{
    public class FieldDelcaration : Declaration
    {
        public FieldDelcaration(string name, System.Type type) : base(name)
        {
            ResolvedType = type;
        }

        public FieldDelcaration(string name, TypeName typeName) : base(name, typeName)
        {
        }
    }
}
