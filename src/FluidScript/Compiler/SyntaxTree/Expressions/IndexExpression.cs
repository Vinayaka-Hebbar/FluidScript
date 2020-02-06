using FluidScript.Reflection.Emit;

namespace FluidScript.Compiler.SyntaxTree
{
    /// <summary>
    /// Indexer value[exps]
    /// </summary>
    public sealed class IndexExpression : Expression
    {
        public readonly Expression Target;

        public readonly Expression[] Arguments;

        public System.Reflection.MethodInfo Getter { get; internal set; }

        public System.Reflection.MethodInfo Setter { get; internal set; }

        public IndexExpression(Expression target, Expression[] arguments) : base(ExpressionType.Indexer)
        {
            Target = target;
            Arguments = arguments;
        }

        public override TResult Accept<TResult>(IExpressionVisitor<TResult> visitor)
        {
            return visitor.VisitIndex(this);
        }

        public override void GenerateCode(MethodBodyGenerator generator)
        {
            Target.GenerateCode(generator);
            System.Type type = Target.Type;
            if (type.IsArray)
            {
                Iterate(Arguments, (arg) =>
                {
                    arg.GenerateCode(generator);
                    generator.CallStatic(Utils.Helpers.Integer_to_Int32);
                });
                System.Type elementType = type.GetElementType();
                generator.LoadArrayElement(elementType);
            }
            else
            {

                Iterate(Arguments, (arg) => arg.GenerateCode(generator));
                //todo indexer argument convert
                generator.Call(Getter);
            }
        }

        public override string ToString()
        {
            return string.Concat(Target, "[", string.Join(",", System.Linq.Enumerable.Select(Arguments, arg => arg.ToString())), "]");
        }
    }
}
