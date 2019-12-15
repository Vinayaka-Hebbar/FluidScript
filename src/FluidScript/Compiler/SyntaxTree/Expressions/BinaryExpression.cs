using FluidScript.Reflection.Emit;
using System.Collections.Generic;

namespace FluidScript.Compiler.SyntaxTree
{
    public sealed class BinaryExpression : Expression
    {
        public readonly Expression Left;
        public readonly Expression Right;

        public System.Reflection.MethodInfo Method { get; internal set; }

        public Conversion[] Conversions { get; internal set; }

        public BinaryExpression(Expression left, Expression right, ExpressionType opCode) : base(opCode)
        {
            Left = left;
            Right = right;
        }

        public override IEnumerable<Node> ChildNodes() => Childs(Left, Right);

        public override TResult Accept<TResult>(IExpressionVisitor<TResult> visitor)
        {
            return visitor.VisitBinary(this);
        }

        public override void GenerateCode(MethodBodyGenerator generator)
        {
            var leftType = Left.Type;
            var rightType = Right.Type;
            //todo conversion 
            if (Method == null)
                throw new System.NullReferenceException(nameof(Method));
            var parameters = Method.GetParameters();
            Left.GenerateCode(generator);
            var first = parameters[0].ParameterType;
            if (leftType != first)
                EmitConvertion.Convert(generator, leftType, first);
            Right.GenerateCode(generator);
            var second = parameters[1].ParameterType;
            if (rightType != second)
                EmitConvertion.Convert(generator, rightType, second);
            generator.Call(Method);
            //case ExpressionType.Less:
            //    LoadValues(generator);
            //    generator.CompareLessThan();
            //    break;
            //case ExpressionType.LessEqual:
            //    LoadValues(generator);
            //    if (Left.GetRuntimeType(generator) == RuntimeType.Double || Right.GetRuntimeType(generator) == RuntimeType.Double)
            //        generator.CompareGreaterThanUnsigned();
            //    else
            //        generator.CompareGreaterThan();
            //    generator.LoadInt32(0);
            //    generator.CompareEqual();
            //    break;
            //case ExpressionType.Greater:
            //    LoadValues(generator);
            //    generator.CompareGreaterThan();
            //    break;
            //case ExpressionType.GreaterEqual:
            //    LoadValues(generator);
            //    if (Left.GetRuntimeType(generator) == RuntimeType.Double || Right.GetRuntimeType(generator) == RuntimeType.Double)
            //        generator.CompareLessThanUnsigned();
            //    else
            //        generator.CompareLessThan();
            //    generator.LoadInt32(0);
            //    generator.CompareEqual();
            //    break;
        }

        public override string ToString()
        {
            var operation = string.Empty;
            switch (NodeType)
            {
                case ExpressionType.Plus:
                    operation = "+";
                    break;
                case ExpressionType.Minus:
                    operation = "-";
                    break;
                case ExpressionType.Multiply:
                    operation = "*";
                    break;
                case ExpressionType.Divide:
                    operation = "/";
                    break;
                case ExpressionType.Percent:
                    operation = "%";
                    break;
                case ExpressionType.Circumflex:
                    operation = "^";
                    break;
                case ExpressionType.EqualEqual:
                    operation = "==";
                    break;
                case ExpressionType.BangEqual:
                    operation = "!=";
                    break;
                case ExpressionType.Less:
                    operation = "<";
                    break;
                case ExpressionType.LessEqual:
                    operation = "<=";
                    break;
                case ExpressionType.LessLess:
                    operation = "<<";
                    break;
                case ExpressionType.Greater:
                    operation = ">";
                    break;
                case ExpressionType.GreaterEqual:
                    operation = ">=";
                    break;
                case ExpressionType.GreaterGreater:
                    operation = ">>";
                    break;
                case ExpressionType.And:
                    operation = "&";
                    break;
                case ExpressionType.AndAnd:
                    operation = "&&";
                    break;
                case ExpressionType.Or:
                    operation = "|";
                    break;
                case ExpressionType.OrOr:
                    operation = "||";
                    break;
            }
            return string.Concat(Left.ToString(), operation, Right.ToString());
        }
    }
}
