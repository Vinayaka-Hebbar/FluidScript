namespace FluidScript.Compiler.SyntaxTree
{
    public class QualifiedExpression : Expression
    {
        public readonly Expression Target;
        public readonly NameExpression Identifier;

        public QualifiedExpression(Expression target, NameExpression identifier, ExpressionType opCode) : base(opCode)
        {
            Target = target;
            Identifier = identifier;
        }

        public override string ToString()
        {
            if (NodeType == ExpressionType.QualifiedNamespace)
            {
                return Target.ToString() + '.' + Identifier;
            }
            return Identifier.ToString();
        }

        public override TReturn Accept<TReturn>(INodeVisitor<TReturn> visitor)
        {
            return visitor.VisitQualifiedExpression(this);
        }
    }
}
