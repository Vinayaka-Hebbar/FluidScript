using System.Linq;

namespace FluidScript.Compiler.SyntaxTree
{
    public class AnonymousFunctionExpression : Expression, IFunctionExpression
    {
        public AnonymousFunctionExpression(Node[] arguments, Statement body) : base(NodeType.Function)
        {
            Arguments = arguments;
            Body = body;
        }

        public Node[] Arguments { get; }

        public Statement Body { get; }

        public FunctionPartBuilder GetPartBuilder()
        {
            return new FunctionPartBuilder(Arguments.Length, Invoke, CodeScope.Local);
        }

        private Object Invoke(NodeVisitor visitor, Node[] args)
        {
            visitor = new NodeVisitor(visitor);
            return visitor.VisitFunction(this, args.Select(arg => arg.Accept(visitor)).ToArray());
        }

        public override TReturn Accept<TReturn>(INodeVisitor<TReturn> visitor)
        {
            return visitor.VisitAnonymousFuntion(this);
        }
    }
}
