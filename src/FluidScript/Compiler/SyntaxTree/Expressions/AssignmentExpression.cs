using FluidScript.Reflection.Emit;
using System.Linq;
using System.Runtime.InteropServices;

namespace FluidScript.Compiler.SyntaxTree
{
    public sealed class AssignmentExpression : Expression
    {
        public readonly Expression Left;
        public readonly Expression Right;
        public AssignmentExpression(Expression left, Expression right) : base(ExpressionType.Equal)
        {
            Left = left;
            Right = right;
        }

        public override TResult Accept<TResult>(IExpressionVisitor<TResult> visitor)
        {
            return visitor.VisitAssignment(this);
        }

        public override void GenerateCode(MethodBodyGenerator generator)
        {
            //todo index implementation pending
            if (Left.NodeType == ExpressionType.Identifier)
            {
                var exp = (NameExpression)Left;
                Binding binding = exp.Binding;
                if (binding.IsMember && binding.IsStatic == false)
                    generator.LoadArgument(0);
                Right.GenerateCode(generator);
                binding.GenerateSet(generator);
            }
            else if (Left.NodeType == ExpressionType.MemberAccess)
            {
                var exp = (MemberExpression)Left;
                exp.Target.GenerateCode(generator);
                Right.GenerateCode(generator);
                exp.Binding.GenerateSet(generator);
            }else if(Left.NodeType == ExpressionType.Indexer)
            {
                var exp = (IndexExpression)Left;
                exp.Target.GenerateCode(generator);
                System.Type type = exp.Target.Type;
                if (type.IsArray)
                {
                    Iterate(exp.Arguments, (arg) =>
                    {
                        arg.GenerateCode(generator);
                        generator.CallStatic(Helpers.Integer_to_Int32);
                    });
                    Right.GenerateCode(generator);
                    System.Type elementType = type.GetElementType();
                    generator.StoreArrayElement(elementType);
                }
                else
                {

                    Iterate(exp.Arguments, (arg) => arg.GenerateCode(generator));
                    //todo indexer argument convert
                    generator.Call(exp.Setter);
                }
            }
        }

        public override string ToString()
        {
            return string.Concat(Left.ToString(), "=", Right.ToString());
        }
    }
}
