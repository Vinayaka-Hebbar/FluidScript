using System.Runtime.InteropServices;
using FluidScript.Reflection.Emit;

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

        public override void GenerateCode(MethodBodyGenerator generator)
        {
            generator.LoadNull();
        }
    }
}
