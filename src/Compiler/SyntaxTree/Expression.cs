using FluidScript.Compiler.Emit;

namespace FluidScript.Compiler.SyntaxTree
{
    public abstract class Expression : Node
    {
        internal static readonly Expression Empty = new EmptyExpression();
        protected System.Type ResolvedType = null;

        public Expression(ExpressionType nodeType)
        {
            NodeType = nodeType;
        }
        public ExpressionType NodeType { get; }

        public string TypeName { get; }

        public virtual System.Type Type => ResolvedType;
        public virtual PrimitiveType ResultType { get; } = PrimitiveType.Any;

        public override object GetValue()
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
        public EmptyExpression() : base(ExpressionType.Unknown)
        {
        }

        public override TReturn Accept<TReturn>(INodeVisitor<TReturn> visitor)
        {
            return default;
        }

        public override void GenerateCode(ILGenerator generator, OptimizationInfo info)
        {
            generator.NoOperation();
        }
    }
}
