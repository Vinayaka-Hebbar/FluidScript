using FluidScript.Runtime;

namespace FluidScript.Compiler.SyntaxTree
{
    public class NewExpression : Expression
    {
        public readonly TypeSyntax TypeSyntax;

        public readonly NodeList<Expression> Arguments;

        public ArgumentConversions Conversions { get; internal set; }

        public System.Reflection.ConstructorInfo Constructor { get; internal set; }

        public NewExpression(TypeSyntax typeName, NodeList<Expression> arguments) : base(ExpressionType.New)
        {
            TypeSyntax = typeName;
            Arguments = arguments;
        }

        public override TResult Accept<TResult>(IExpressionVisitor<TResult> visitor)
        {
            return visitor.VisitNew(this);
        }

        public override void GenerateCode(Emit.MethodBodyGenerator generator, Emit.MethodGenerateOption option = 0)
        {
            InvocationExpression.EmitArguments(generator, Arguments, Conversions);
            generator.NewObject(Constructor);
        }
    }
}
