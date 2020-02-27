using FluidScript.Compiler.Emit;
using System.Collections.Generic;

namespace FluidScript.Compiler.SyntaxTree
{
    public sealed class BinaryExpression : Expression
    {
        public readonly Expression Left;
        public readonly Expression Right;

        public System.Reflection.MethodInfo Method { get; internal set; }

        public Binders.ArgumentBinderList Bindings { get; internal set; }

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
            //todo conversion 
            if (Method == null)
                throw new System.NullReferenceException(nameof(Method));
            var bindings = Bindings;
            Left.GenerateCode(generator);
            var first = bindings.BindingAt(0);
            if (first != null)
                first.Generate(generator);
            Right.GenerateCode(generator);
            var second = bindings.BindingAt(1);
            if (second != null)
                second.Generate(generator);
            generator.Call(Method);
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
