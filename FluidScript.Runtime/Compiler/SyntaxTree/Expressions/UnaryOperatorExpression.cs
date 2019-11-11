using FluidScript.Compiler.Emit;

namespace FluidScript.Compiler.SyntaxTree
{
    public class UnaryOperatorExpression : Expression
    {
        public readonly Expression Operand;

        public UnaryOperatorExpression(Expression operand, ExpressionType opcode)
            : base(opcode)
        {
            Operand = operand;
        }

        protected override void ResolveType(OptimizationInfo info)
        {
            ResolvedType = Operand.ResultType(info);
            ResolvedPrimitiveType = Operand.PrimitiveType(info);
        }

        public override RuntimeObject Evaluate()
        {
            var result = Operand.Evaluate();
            switch (NodeType)
            {
                case ExpressionType.Parenthesized:
                    return result;
                case ExpressionType.Bang:
                    return !result;
                case ExpressionType.Plus:
                    return +result;
                case ExpressionType.Minus:
                    return -result;
                case ExpressionType.Out:
                default:
                    var expression = (NameExpression)Operand;
                    if (expression.NodeType == ExpressionType.Identifier)
                    {
                        var value = result;
                        switch (NodeType)
                        {
                            case ExpressionType.PostfixPlusPlus:
                                value = value + 1;
                                break;
                            case ExpressionType.PostfixMinusMinus:
                                value = value - 1;
                                break;
                            case ExpressionType.PrefixPlusPlus:
                                result = value = value + 1;
                                break;
                            case ExpressionType.PrefixMinusMinus:
                                result = value = value - 1;
                                break;
                            case ExpressionType.Bang:
                                result = !value;
                                break;
                        }
                        expression.Set(value);
                    }
                    return result;
            }
        }

        public override void GenerateCode(ILGenerator generator, MethodOptimizationInfo info)
        {
            if (NodeType == ExpressionType.Parenthesized)
                Operand.GenerateCode(generator, info);
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
