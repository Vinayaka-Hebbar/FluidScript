using System.Linq;

namespace FluidScript.Compiler.SyntaxTree
{
    public sealed class AnonymousFunctionExpression : Expression
    {
        public readonly TypeParameter[] Parameters;

        public readonly TypeSyntax ReturnType;

        public readonly BlockStatement Body;

        public AnonymousFunctionExpression(TypeParameter[] parameters, TypeSyntax returnType, BlockStatement body) : base(ExpressionType.Function)
        {
            Parameters = parameters;
            ReturnType = returnType;
            Body = body;
        }

        public override string ToString()
        {
            //todo return type
            return string.Concat("(", string.Join(",", Parameters.Select(arg => arg.ToString())), "):any");
        }
    }
}
