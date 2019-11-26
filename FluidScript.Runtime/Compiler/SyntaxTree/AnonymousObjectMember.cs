namespace FluidScript.Compiler.SyntaxTree
{
    public class AnonymousObjectMember : Node
    {
        public readonly string Name;
        public readonly Expression Expression;

        public AnonymousObjectMember(string name, Expression expression)
        {
            Name = name;
            Expression = expression;
        }

        public RuntimeObject Evaluate(RuntimeObject instance)
        {
            return Expression.Evaluate(instance);
        }

        public override string ToString()
        {
            return string.Concat(Name, ":", Expression);
        }
    }
}
