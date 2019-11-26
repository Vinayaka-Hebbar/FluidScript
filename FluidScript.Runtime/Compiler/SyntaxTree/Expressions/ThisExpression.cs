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
    }
}
