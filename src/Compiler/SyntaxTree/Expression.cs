namespace FluidScript.Compiler.SyntaxTree
{
    public class Expression : Node
    {
        internal static readonly Expression Empty = new EmptyExpression();
        protected System.Type ResolvedType = null;
        /// <summary>
        /// todo for resolve result type assign resolve type if not any
        /// </summary>
        protected PrimitiveType ResolvedPrimitiveType;

        public Expression(ExpressionType nodeType)
        {
            NodeType = nodeType;
        }
        public ExpressionType NodeType { get; }

        public virtual Emit.TypeName TypeName
        {
            get
            {
                if (ResolvedType == null)
                    return Emit.TypeName.Empty;
                return new Emit.TypeName(ResolvedType);
            }
        }

        public virtual System.Type ResultType(System.Type declaredType) => ResolvedType;
        public virtual PrimitiveType PrimitiveType(System.Type declaredType) => ResolvedPrimitiveType;

        public virtual void GenerateCode(Emit.ILGenerator generator, Emit.MethodOptimizationInfo info)
        {
            generator.NoOperation();
        }

    }

    internal sealed class EmptyExpression : Expression
    {
        public EmptyExpression() : base(ExpressionType.Unknown)
        {
        }
    }
}
