namespace FluidScript.Compiler.SyntaxTree
{
    public class LabeledStatement : Statement
    {
        public readonly string Name;
        public readonly Expression Target;

        public LabeledStatement(string name, Expression target): base(StatementType.Labeled)
        {
            Name = name;
            Target = target;
        }

        public override string ToString()
        {
            return string.Concat(Name, ":", Target.ToString());
        }
    }
}
