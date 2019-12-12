namespace FluidScript.Compiler.SyntaxTree
{
    public class Expression : Node
    {
        internal static readonly Expression Empty = new EmptyExpression();
#if Runtime
        public static readonly Expression Null = new NullExpression();

        public static readonly Expression Undefined = new NullExpression();
#else
        public static readonly LiteralExpression Null = new LiteralExpression(null, RuntimeType.Undefined);
#endif

        public Expression(ExpressionType nodeType)
        {
            NodeType = nodeType;
        }

        public ExpressionType NodeType { get; }

        public System.Type Type { get; protected internal set; }

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
