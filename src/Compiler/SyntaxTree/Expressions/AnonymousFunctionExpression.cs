using System.Linq;

namespace FluidScript.Compiler.SyntaxTree
{
    public class AnonymousFunctionExpression : Expression
    {
        public AnonymousFunctionExpression(Node[] arguments, Statement body) : base(ExpressionType.Function)
        {
            Arguments = arguments;
            Body = body;
        }

        public Node[] Arguments { get; }

        public Statement Body { get; }
    }
}
