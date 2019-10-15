using System.Linq;

namespace FluidScript.SyntaxTree.Expressions
{
    public class AnonymousFunctionExpression : IExpression, IFunctionExpression
    {
        public AnonymousFunctionExpression(IExpression[] arguments, Statement body)
        {
            Arguments = arguments;
            Body = body;
        }

        public Expression.Operation Kind => Expression.Operation.Function;

        public IExpression[] Arguments { get; }

        public Statement Body { get; }

        public FunctionPartBuilder GetPartBuilder()
        {
            return new FunctionPartBuilder(Arguments.Length, Invoke, Scope.Local);
        }

        private Object Invoke(NodeVisitor visitor, IExpression[] args)
        {
            visitor = new NodeVisitor(visitor);
            return visitor.VisitFunction(this, args.Select(arg => arg.Accept(visitor)).ToArray());
        }

        public TReturn Accept<TReturn>(INodeVisitor<TReturn> visitor) where TReturn : IRuntimeObject
        {
            return visitor.VisitAnonymousFuntion(this);
        }
    }
}
