namespace FluidScript.SyntaxTree.Expressions
{
    public class QualifiedExpression : Expression
    {
        public readonly IExpression Target;
        public readonly IdentifierExpression Identifier;

        public QualifiedExpression(IExpression target, IdentifierExpression identifier, Operation opCode) : base(opCode)
        {
            Target = target;
            Identifier = identifier;
        }

        public override string ToString()
        {
            if (OpCode == Operation.QualifiedNamespace)
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
