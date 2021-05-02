namespace FluidScript.Compiler.SyntaxTree
{
    public class SizeOfExpression : Expression
    {
        public readonly Expression Value;

        public SizeOfExpression(Expression value) : base(ExpressionType.Identifier)
        {
            Value = value;
        }

        public override System.Collections.Generic.IEnumerable<Node> ChildNodes()
        {
            return Childs(Value);
        }

        public override TResult Accept<TResult>(IExpressionVisitor<TResult> visitor)
        {
            return visitor.VisitSizeOf(this);
        }

        public override string ToString()
        {
            return string.Concat("sizeof(", Value, ")");
        }
    }
}
