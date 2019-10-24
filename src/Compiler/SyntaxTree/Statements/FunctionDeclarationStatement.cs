using FluidScript.Compiler.Reflection;

namespace FluidScript.Compiler.SyntaxTree
{
    public class FunctionDeclarationStatement : Statement
    {
        public readonly string Name;
        public readonly ParameterInfo[] Arguments;

        public FunctionDeclarationStatement(string name, ParameterInfo[] arguments) : base(NodeType.Declaration)
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
