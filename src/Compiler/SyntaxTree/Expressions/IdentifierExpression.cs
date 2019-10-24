namespace FluidScript.Compiler.SyntaxTree
{
    public class IdentifierExpression : Expression
    {
        public readonly string Id;

        public IdentifierExpression(string id, NodeType opCode) : base(opCode)
        {
            Id = id;
        }

        public override string ToString()
        {
            return Id;
        }

        public IFunction GetFunction(IOperationContext context)
        {
            return context.Functions[Id];
        }

        public override TReturn Accept<TReturn>(INodeVisitor<TReturn> visitor)
        {
            return visitor.VisitIdentifier(this);
        }
    }
}
