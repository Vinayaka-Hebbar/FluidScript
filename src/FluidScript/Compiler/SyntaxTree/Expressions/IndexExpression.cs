using FluidScript.Compiler.Emit;

namespace FluidScript.Compiler.SyntaxTree
{
    /// <summary>
    /// Indexer value[exps]
    /// </summary>
    public sealed class IndexExpression : Expression
    {
        public readonly Expression Target;

        public readonly INodeList<Expression> Arguments;

        /// <summary>
        /// Argument convert list
        /// </summary>
        public Binders.ArgumenConversions Conversions { get; set; }

        public System.Reflection.MethodInfo Getter { get; set; }

        public System.Reflection.MethodInfo Setter { get; set; }

        public IndexExpression(Expression target, NodeList<Expression> arguments) : base(ExpressionType.Indexer)
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
                    generator.CallStatic(Utils.ReflectionHelpers.IntegerToInt32);
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
