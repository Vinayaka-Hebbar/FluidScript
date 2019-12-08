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
        private System.Type resolvedType = null;
        /// <summary>
        /// todo for resolve result type assign resolve type if not any
        /// </summary>
        private RuntimeType resolvedRuntimeType = RuntimeType.Undefined;

        public Expression(ExpressionType nodeType)
        {
            NodeType = nodeType;
        }

        public ExpressionType NodeType { get; }

        public System.Type Type => resolvedType;

        protected internal System.Type ResolvedType
        {
            get => resolvedType;
            set
            {
                resolvedType = value;
                if (value.IsPrimitive)
                    resolvedRuntimeType = Reflection.Emit.TypeUtils.GetRuntimeType(value);
                else if (value.FullName == "FluidScript.String")
                    resolvedRuntimeType = RuntimeType.String;
                else
                    resolvedRuntimeType = RuntimeType.Any;
                if (value.IsArray)
                    resolvedRuntimeType |= RuntimeType.Array;
            }
        }

        protected RuntimeType ResolvedRuntimeType
        {
            get => resolvedRuntimeType;
            set
            {
                resolvedType = Reflection.Emit.TypeUtils.ToType(value);
                resolvedRuntimeType = value;
            }
        }

#if Runtime
        /// <summary>
        /// Get Value for the scope
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public virtual RuntimeObject Evaluate(RuntimeObject instance)
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

        /// <summary>
        /// Optimizes expression for emit or others
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="visitor"></param>
        /// <returns></returns>
        public virtual TResult Accept<TResult>(IExpressionVisitor<TResult> visitor)
        {
            return default;
        }

        public virtual void GenerateCode(Reflection.Emit.MethodBodyGenerator generator)
        {
            generator.NoOperation();
        }
    }

    internal sealed class EmptyExpression : Expression
    {
        public EmptyExpression() : base(ExpressionType.Unknown)
        {
        }

        public override string ToString()
        {
            return string.Empty;
        }
    }
}
