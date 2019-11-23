using System.Runtime.InteropServices;

namespace FluidScript.Compiler.SyntaxTree
{
    public class Expression : Node
    {
        internal static readonly Expression Empty = new EmptyExpression();
#if Runtime
        public static readonly Expression Null = new NullExpression(RuntimeObject.Null);

        public static readonly Expression Undefined = new NullExpression(RuntimeObject.Undefined);
#else
        public static readonly LiteralExpression Null = new LiteralExpression(null, RuntimeType.Undefined);
#endif
        protected System.Type ResolvedType = null;
        /// <summary>
        /// todo for resolve result type assign resolve type if not any
        /// </summary>
        protected RuntimeType ResolvedPrimitiveType = FluidScript.RuntimeType.Undefined;

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
                    return Emit.TypeName.Any;
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

        public RuntimeType PrimitiveType(Emit.OptimizationInfo info)
        {
            if (ResolvedPrimitiveType == FluidScript.RuntimeType.Undefined)
                ResolveType(info);
            return ResolvedPrimitiveType;
        }

#if Runtime
        /// <summary>
        /// Get Value for the scope
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public virtual RuntimeObject Evaluate([Optional]RuntimeObject instance)
        {
            return RuntimeObject.Null;
        }

        /// <summary>
        /// For static evaluation
        /// </summary>
        /// <param name="prototype"></param>
        /// <returns></returns>
        public virtual RuntimeObject Evaluate(Metadata.Prototype prototype)
        {
            return RuntimeObject.Null;
        }
#else
        public virtual object Evaluate()
        {
            return null;
        }
#endif

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
