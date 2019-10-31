using FluidScript.Compiler.Emit;

namespace FluidScript.Compiler.SyntaxTree
{
    public class ArgumentExpression : Node
    {
        public readonly Object Value;

        public ArgumentExpression(Object value) 
        {
            Value = value;
        }

        public override TReturn Accept<TReturn>(INodeVisitor<TReturn> visitor)
        {
            return visitor.VisitArgument(this);
        }

        public override Object GetValue()
        {
            return Value;
        }

        public override void GenerateCode(ILGenerator generator, OptimizationInfo info)
        {
            throw new System.NotImplementedException();
        }
    }
}
