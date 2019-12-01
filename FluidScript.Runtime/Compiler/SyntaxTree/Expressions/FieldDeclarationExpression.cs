using System.Runtime.InteropServices;

namespace FluidScript.Compiler.SyntaxTree
{
    public class FieldDeclarationExpression : DeclarationExpression
    {

        public readonly TypeSyntax Type;
        public readonly Expression Value;

        public FieldDeclarationExpression(string name, TypeSyntax type, Expression value) : base(name)
        {
            Type = type;
            Value = value;

        }

#if Runtime
        public override RuntimeObject Evaluate(RuntimeObject instance)
        {
            return Value.Evaluate(instance);
        }
#endif
    }
}
