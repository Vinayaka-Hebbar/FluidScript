namespace FluidScript.Compiler.SyntaxTree
{
    /// <summary>
    /// Labeled statement 
    /// </summary>
    public class LabeledStatement : Statement
    {
        /// <summary>
        /// Label name
        /// </summary>
        public readonly string Name;
        /// <summary>
        /// Target
        /// </summary>
        public readonly Expression Target;

        /// <summary>
        /// Initializes new <see cref="LabeledStatement"/>
        /// </summary>
        public LabeledStatement(string name, Expression target): base(StatementType.Labeled)
        {
            Name = name;
            Target = target;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Concat(Name, ":", Target.ToString());
        }
    }
}
