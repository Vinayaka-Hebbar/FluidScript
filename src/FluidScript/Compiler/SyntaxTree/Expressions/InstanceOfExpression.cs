namespace FluidScript.Compiler.SyntaxTree
{
    public class InstanceOfExpression : Expression
    {

        public readonly Expression Target;

        public readonly TypeSyntax TypeSyntax;

        public InstanceOfExpression(Expression target, TypeSyntax typeSyntax) : base(ExpressionType.InstanceOf)
        {
            Target = target;
            TypeSyntax = typeSyntax;
            Type = typeof(bool);
        }

        public override TResult Accept<TResult>(IExpressionVisitor<TResult> visitor)
        {
            return visitor.VisitInstanceOf(this);
        }

        public override void GenerateCode(Emit.MethodBodyGenerator generator, Emit.MethodGenerateOption option)
        {
            Target.GenerateCode(generator);
            if (TypeSyntax is null || (TypeSyntax is RefTypeSyntax refType && refType.Name.Equals(LiteralExpression.NullString)))
            {
                generator.LoadNull();
                generator.CompareEqual();
            }
            else
            {
                generator.IsInstance(TypeSyntax.Type);
                generator.LoadNull();
                generator.CompareGreaterThanUnsigned();
            }
        }

        public override string ToString()
        {
            return $"{Target} instanceOf {(TypeSyntax == null ? LiteralExpression.NullString : TypeSyntax.ToString())}";
        }
    }
}
