namespace FluidScript.SyntaxTree.Expressions
{
    public class InvocationExpression : Expression
    {
        public readonly IExpression Target;
        public readonly IExpression[] Arguments;

        public InvocationExpression(IExpression target, IExpression[] arguments, Operation opCode) : base(opCode)
        {
            Target = target;
            Arguments = arguments;
        }

        public override TReturn Accept<TReturn>(INodeVisitor<TReturn> visitor)
        {
            return visitor.VisitInvocation(this);
        }
    }
}
