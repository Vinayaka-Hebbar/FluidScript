using System.Runtime.InteropServices;

namespace FluidScript.Compiler.SyntaxTree
{
    public class NullExpression : Expression
    {
#if Runtime
        public readonly RuntimeObject Value;

        public NullExpression(RuntimeObject value) : base(ExpressionType.Literal)
        {
            Value = value;
        }
#endif
        public NullExpression() : base(ExpressionType.Literal)
        {
        }


#if Runtime
        public override RuntimeObject Evaluate([Optional] RuntimeObject instance)
        {
            return Value;
        }
#endif
    }
}
