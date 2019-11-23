using System.Runtime.InteropServices;
using FluidScript.Compiler.Metadata;

namespace FluidScript.Compiler.SyntaxTree
{
    public class ConstantDeclarationExpression : DeclarationExpression
    {
        public readonly RuntimeObject Value;

        public ConstantDeclarationExpression(string name, RuntimeObject value) : base(name)
        {
            Value = value;
        }

        public override RuntimeObject Evaluate(Prototype prototype)
        {
            return Value;
        }

        public override RuntimeObject Evaluate([Optional] RuntimeObject instance)
        {
            return Value;
        }

        public override string ToString()
        {
            return string.Concat(Name, "=", Value);
        }
    }
}
