namespace FluidScript.SyntaxTree.Statements
{
    public class FunctionDeclarationStatement : Statement
    {
        public readonly string Name;
        public readonly IExpression[] Arguments;

        public FunctionDeclarationStatement(string name, IExpression[] arguments) : base(Operation.Declaration)
        {
            Name = name;
            Arguments = arguments;
        }

        public override TReturn Accept<TReturn>(INodeVisitor<TReturn> visitor)
        {
            return visitor.VisitVoid();
        }
    }
}
