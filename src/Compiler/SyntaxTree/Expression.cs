using FluidScript.Compiler.Emit;

namespace FluidScript.Compiler.SyntaxTree
{
    public class Expression : Node
    {
        internal static readonly Expression Empty = new EmptyExpression();
        protected System.Type ResolvedType = null;

        public Expression(ExpressionType nodeType)
        {
            NodeType = nodeType;
        }
        public ExpressionType NodeType { get; }

        public virtual string TypeName
        {
            get
            {
                if (Type == null)
                    return string.Empty;
                return Type.FullName;
            }
        }

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

        public override void GenerateCode(ILGenerator generator, OptimizationInfo info)
        {
            generator.NoOperation();
        }
    }
}
