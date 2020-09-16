using FluidScript.Compiler.Emit;
using System;
using System.Collections.Generic;

namespace FluidScript.Compiler.SyntaxTree
{
    public sealed class BinaryExpression : Expression
    {
        internal const string OpAddition = "op_Addition";

        public readonly Expression Left;
        public readonly Expression Right;

        public System.Reflection.MethodInfo Method { get; internal set; }

        /// <summary>
        /// Argument convert list
        /// </summary>
        public Runtime.ArgumentConversions Conversions { get; internal set; }


        public BinaryExpression(Expression left, Expression right, ExpressionType opCode) : base(opCode)
        {
            Left = left;
            Right = right;
        }

        public override IEnumerable<Node> ChildNodes() => Childs(Left, Right);

        

        public string MethodName
        {
            get
            {
                string opName = null;
                switch (NodeType)
                {
                    case ExpressionType.Plus:
                        opName = OpAddition;
                        break;
                    case ExpressionType.Minus:
                        opName = "op_Subtraction";
                        break;
                    case ExpressionType.Multiply:
                        opName = "op_Multiply";
                        break;
                    case ExpressionType.Divide:
                        opName = "op_Division";
                        break;
                    case ExpressionType.Percent:
                        opName = "op_Modulus";
                        break;
                    case ExpressionType.BangEqual:
                        opName = "op_Inequality";
                        break;
                    case ExpressionType.EqualEqual:
                        opName = "op_Equality";
                        break;
                    case ExpressionType.Greater:
                        opName = "op_GreaterThan";
                        break;
                    case ExpressionType.GreaterGreater:
                        opName = "op_RightShift";
                        break;
                    case ExpressionType.GreaterEqual:
                        opName = "op_GreaterThanOrEqual";
                        break;
                    case ExpressionType.Less:
                        opName = "op_LessThan";
                        break;
                    case ExpressionType.LessLess:
                        opName = "op_LeftShift";
                        break;
                    case ExpressionType.LessEqual:
                        opName = "op_LessThanOrEqual";
                        break;
                    case ExpressionType.And:
                        opName = "op_BitwiseAnd";
                        break;
                    case ExpressionType.Or:
                        opName = "op_BitwiseOr";
                        break;
                    case ExpressionType.Circumflex:
                        opName = "op_ExclusiveOr";
                        break;
                }
                return opName;
            }
        }

        public override TResult Accept<TResult>(IExpressionVisitor<TResult> visitor)
        {
            return visitor.VisitBinary(this);
        }

        public override void GenerateCode(MethodBodyGenerator generator, MethodCompileOption options)
        {
            if (Method != null)
            {
                var conversions = Conversions;
                Left.GenerateCode(generator);
                var first = conversions[0];
                if (first != null)
                {
                    generator.EmitConvert(first);
                }
                Right.GenerateCode(generator);
                var second = conversions[1];
                if (second != null)
                {
                    generator.EmitConvert(second);
                }
                generator.Call(Method);
                return;
            }
            throw new InvalidOperationException("Operator method missing");
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
                case ExpressionType.StarStar:
                    operation = "**";
                    break;
                case ExpressionType.Comma:
                    operation = ",";
                    break;
            }
            return string.Concat(Left.ToString(), operation, Right.ToString());
        }
    }
}
