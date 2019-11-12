namespace FluidScript.Compiler.SyntaxTree
{
    public class ThisExpression : Expression
    {
        private readonly Metadata.Prototype prototype;

        public ThisExpression(Metadata.Prototype prototype) : base(ExpressionType.This)
        {
            this.prototype = prototype;
        }

        public override RuntimeObject Evaluate()
        {
            return prototype;
        }
    }
}
