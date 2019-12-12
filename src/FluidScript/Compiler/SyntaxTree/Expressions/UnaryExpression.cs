using FluidScript.Reflection.Emit;

namespace FluidScript.Compiler.SyntaxTree
{
    public sealed class UnaryExpression : Expression
    {
        public readonly Expression Operand;

        public UnaryExpression(Expression operand, ExpressionType opcode)
            : base(opcode)
        {
            Operand = operand;
        }

        public override TResult Accept<TResult>(IExpressionVisitor<TResult> visitor)
        {
            return visitor.VisitUnary(this);
        }

        public override void GenerateCode(MethodBodyGenerator generator)
        {
            if (NodeType == ExpressionType.Parenthesized)
                Operand.GenerateCode(generator);
        }

        public override string ToString()
        {
            var result = Operand.ToString();
            switch (NodeType)
            {
                case ExpressionType.Parenthesized:
                    return string.Concat('(', result, ')');
                case ExpressionType.PostfixPlusPlus:
                    return string.Concat(result, "++");
                case ExpressionType.PostfixMinusMinus:
                    return string.Concat(result, "--");
                case ExpressionType.PrefixPlusPlus:
                    return string.Concat("++", result);
                case ExpressionType.PrefixMinusMinus:
                    return string.Concat("--", result);
                case ExpressionType.Bang:
                    return string.Concat("!", result);
                case ExpressionType.Plus:
                    return string.Concat("+", result);
                case ExpressionType.Minus:
                    return string.Concat("-", result);
                case ExpressionType.Out:
                default:
                    return string.Concat("out ", result); ;
            }
        }
    }
}
