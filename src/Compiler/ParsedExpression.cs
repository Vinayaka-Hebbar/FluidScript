using FluidScript.Compiler.SyntaxTree;

namespace FluidScript.Compiler
{
    public sealed class ParsedExpression
    {
        public readonly Expression Expression;
        public readonly IOperationContext Context;

        public ParsedExpression(IOperationContext context, Expression expression)
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
