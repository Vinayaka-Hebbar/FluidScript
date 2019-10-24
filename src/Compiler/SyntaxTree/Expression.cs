using FluidScript.Compiler.Emit;

namespace FluidScript.Compiler.SyntaxTree
{
    public abstract class Expression : Node
    {
        internal static readonly Expression Empty = new EmptyExpression();

        public Expression(NodeType opCode) : base(opCode)
        {
            
        }

        public virtual ObjectType ResultType => ObjectType.Object;

        public override Object GetValue()
        {
            return null;
        }
        public override void GenerateCode(ILGenerator generator, OptimizationInfo info)
        {
            generator.NoOperation();
        }

    }

    internal sealed class EmptyExpression : Expression
    {
        public EmptyExpression() : base(NodeType.Unknown)
        {
        }

        public override TReturn Accept<TReturn>(INodeVisitor<TReturn> visitor)
        {
            return default(TReturn);
        }

        public override void GenerateCode(ILGenerator generator, OptimizationInfo info)
        {
            generator.NoOperation();
        }
    }
}
