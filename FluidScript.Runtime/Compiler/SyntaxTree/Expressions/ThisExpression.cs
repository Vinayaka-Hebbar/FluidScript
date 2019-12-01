using FluidScript.Reflection.Emit;

namespace FluidScript.Compiler.SyntaxTree
{
    public class ThisExpression : Expression
    {
        public ThisExpression() : base(ExpressionType.This)
        {
        }

#if Runtime
        public override RuntimeObject Evaluate(RuntimeObject instance)
        {
            return instance["this"];
        }
#endif

        public override void GenerateCode(MethodBodyGenerator generator)
        {
            ResolvedType = generator.TypeGenerator.Type;
            generator.LoadArgument(0);
        }
    }
}
