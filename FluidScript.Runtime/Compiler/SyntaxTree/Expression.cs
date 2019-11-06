namespace FluidScript.Compiler.SyntaxTree
{
    public class Expression : Node
    {
        internal static readonly Expression Empty = new EmptyExpression();
        protected System.Type ResolvedType = null;
        /// <summary>
        /// todo for resolve result type assign resolve type if not any
        /// </summary>
        protected PrimitiveType ResolvedPrimitiveType = FluidScript.PrimitiveType.Undefined;

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

        internal System.Type ReflectedType()
        {
            return ResolvedType;
        }

        public System.Type ResultType(Emit.OptimizationInfo info)
        {
            if (ResolvedType == null)
                ResolveType(info);
            return ResolvedType;
        }

        protected virtual void ResolveType(Emit.OptimizationInfo info)
        {

        }

        public PrimitiveType PrimitiveType(Emit.OptimizationInfo info)
        {
            if (ResolvedPrimitiveType == FluidScript.PrimitiveType.Undefined)
                ResolveType(info);
            return ResolvedPrimitiveType;
        }

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
