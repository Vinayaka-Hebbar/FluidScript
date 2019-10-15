namespace FluidScript.SyntaxTree
{
    public class ParsedExpression
    {
        public readonly IExpression Expression;
        public readonly IOperationContext Context;

        public ParsedExpression(IOperationContext context, IExpression expression)
        {
            Context = context;
            Expression = expression;
        }

        public Object Evaluate()
        {
            return Expression.Accept(new NodeVisitor(Context));
        }
    }
}
