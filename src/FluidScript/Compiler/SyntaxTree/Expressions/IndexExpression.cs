using FluidScript.Reflection.Emit;

namespace FluidScript.Compiler.SyntaxTree
{
    public sealed class IndexExpression : Expression
    {
        public readonly Expression Target;

        public readonly Expression[] Arguments;

        public System.Reflection.PropertyInfo Indexer { get; internal set; }

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
                    generator.CallStatic(ReflectionHelpers.Integer_to_Int32);
                });
                System.Type elementType = type.GetElementType();
                generator.LoadArrayElement(elementType);
            }
            else
            {

                Iterate(Arguments, (arg) => arg.GenerateCode(generator));
                System.Reflection.MethodInfo indexer = Indexer.GetGetMethod(true);
                //todo indexer argument convert
                generator.Call(indexer);
            }
        }

        public override string ToString()
        {
            return string.Concat(Target, "[", string.Join(",", System.Linq.Enumerable.Select(Arguments, arg => arg.ToString())), "]");
        }
    }
}
