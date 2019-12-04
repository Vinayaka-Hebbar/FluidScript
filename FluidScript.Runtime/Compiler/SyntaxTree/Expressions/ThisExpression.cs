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

        protected override void ResolveType(MethodBodyGenerator generator)
        {
            ResolvedType = generator.TypeGenerator.GetBuilder();
        }

        public override void GenerateCode(MethodBodyGenerator generator)
        {
            ResolvedType = generator.TypeGenerator.GetBuilder();
            generator.LoadArgument(0);
        }

        public override string ToString()
        {
            return "this";
        }
    }
}
