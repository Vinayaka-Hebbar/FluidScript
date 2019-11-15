namespace FluidScript.Compiler.SyntaxTree
{
    public class ThisExpression : Expression
    {
        private readonly Metadata.Prototype prototype;

        public ThisExpression(Metadata.Prototype prototype) : base(ExpressionType.This)
        {
            this.prototype = prototype;
        }

#if Runtime
        public override RuntimeObject Evaluate(RuntimeObject instance)
        {
            return instance["this"];
        }
#endif
    }
}
