using FluidScript.Reflection.Emit;

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

        protected override void ResolveType(MethodBodyGenerator generator)
        {
            switch (NodeType)
            {
                case ExpressionType.Parenthesized:
                    ResolvedType = Operand.ResultType(generator);
                    break;
            }
        }

#if Runtime
        public override RuntimeObject Evaluate(RuntimeObject instance)
        {
            var result = Operand.Evaluate(instance);
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
                            //changes compound value = value + 1
                            case ExpressionType.PostfixPlusPlus:
                                value += 1;
                                break;
                            case ExpressionType.PostfixMinusMinus:
                                value -= 1;
                                break;
                            case ExpressionType.PrefixPlusPlus:
                                result = value += 1;
                                break;
                            case ExpressionType.PrefixMinusMinus:
                                result = value -= 1;
                                break;
                            case ExpressionType.Bang:
                                result = !value;
                                break;
                        }
                        instance[expression.Name] = value;
                    }
                    return result;
            }
        }
#endif

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
