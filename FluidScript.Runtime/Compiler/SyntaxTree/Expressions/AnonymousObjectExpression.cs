using System.Linq;

namespace FluidScript.Compiler.SyntaxTree
{
    public sealed class AnonymousObjectExpression : Expression
    {
        public readonly AnonymousObjectMember[] Members;
        public AnonymousObjectExpression(AnonymousObjectMember[] expressions) : base(ExpressionType.Block)
        {
            Members = expressions;
        }

        public override TResult Accept<TResult>(IExpressionVisitor<TResult> visitor)
        {
            //todo implementation
            return base.Accept(visitor);
        }

        public override string ToString()
        {
            return string.Concat("{", string.Join(",", Members.Select(s => s.ToString())), "}");
        }
    }
}
