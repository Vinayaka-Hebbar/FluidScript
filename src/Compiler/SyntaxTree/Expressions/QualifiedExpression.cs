namespace FluidScript.Compiler.SyntaxTree
{
    public class QualifiedExpression : Expression
    {
        public readonly Expression Target;
        public readonly IdentifierExpression Identifier;

        public QualifiedExpression(Expression target, IdentifierExpression identifier, NodeType opCode) : base(opCode)
        {
            Target = target;
            Identifier = identifier;
        }

        public override string ToString()
        {
            if (NodeType == NodeType.QualifiedNamespace)
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
