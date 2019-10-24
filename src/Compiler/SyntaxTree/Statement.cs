using FluidScript.Compiler.Emit;

namespace FluidScript.Compiler.SyntaxTree
{
    public abstract class Statement : Node
    {
        internal static readonly Statement Empty = new EmptyStatement();

        protected Statement(NodeType opCode) : base(opCode)
        {
        }

        public override Object GetValue()
        {
            return null;
        }

        public override void GenerateCode(ILGenerator generator, OptimizationInfo info)
        {
            generator.NoOperation();
        }

    }

    internal class EmptyStatement : Statement
    {
        public EmptyStatement() : base(NodeType.Unknown)
        {
        }

        public override TReturn Accept<TReturn>(INodeVisitor<TReturn> visitor)
        {
            return visitor.VisitVoid();
        }
    }
}
