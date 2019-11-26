using System.Runtime.InteropServices;
using FluidScript.Compiler.Metadata;

namespace FluidScript.Compiler.SyntaxTree
{
    public class ConstantDeclarationExpression : DeclarationExpression
    {
        public readonly Expression Value;

        public ConstantDeclarationExpression(string name, Expression value) : base(name)
        {
            Value = value;
        }

        public override RuntimeObject Evaluate(Prototype prototype)
        {
            var result = Value.Evaluate(prototype);
            result.Append(Name, result, true);
            return result;
        }

        public override RuntimeObject Evaluate([Optional] RuntimeObject instance)
        {
            return Value.Evaluate(instance.GetPrototype());
        }

        public override string ToString()
        {
            return string.Concat(Name, "=", Value);
        }
    }
}
