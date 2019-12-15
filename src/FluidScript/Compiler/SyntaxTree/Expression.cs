namespace FluidScript.Compiler.SyntaxTree
{
    /// <summary>
    /// Base Expression
    /// </summary>
    public class Expression : Node
    {
        /// <summary>
        /// Empty Expression
        /// </summary>
        public static readonly Expression Empty = new EmptyExpression();

        public static readonly Expression Null = new NullExpression();

        public static readonly Expression Undefined = new NullExpression();

        internal static readonly Expression[] EmptyList = new Expression[0];

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

        /// <summary>
        /// Generates IL code for <see cref="Expression"/>
        /// </summary>
        /// <param name="generator"></param>
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
