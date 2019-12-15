using FluidScript.Reflection.Emit;
using System.Reflection;

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

        /// <summary>
        /// Operator overload method
        /// </summary>
        public MethodInfo Method { get; internal set; }

        /// <summary>
        /// Type conversion of arguments
        /// </summary>
        public Conversion[] Conversions { get; internal set; }

        public override TResult Accept<TResult>(IExpressionVisitor<TResult> visitor)
        {
            return visitor.VisitUnary(this);
        }

        public override void GenerateCode(MethodBodyGenerator generator)
        {
            var operand = Operand;
            if (NodeType == ExpressionType.Parenthesized)
                Operand.GenerateCode(generator);
            else
            {
                if (operand.NodeType == ExpressionType.Identifier)
                {
                    //todo only for get; set; member
                    //i++ or Member++
                    var exp = (NameExpression)operand;
                    Binding binding = exp.Binding;
                    if (binding.IsMember && generator.Method.IsStatic == false)
                        generator.LoadArgument(0);
                    binding.GenerateGet(generator);
                    //todo conversions
                    generator.CallStatic(Method);
                    binding.GenerateSet(generator);
                }
                else if (operand.NodeType == ExpressionType.MemberAccess)
                {
                    var exp = (MemberExpression)operand;
                    exp.Target.GenerateCode(generator);
                    generator.Duplicate();
                    exp.Binding.GenerateGet(generator);
                    //todo conversions
                    generator.CallStatic(Method);
                    exp.Binding.GenerateSet(generator);
                }
            }

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
