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

#if Runtime
        public override RuntimeObject Evaluate([Optional] RuntimeObject instance)
        {
            var value = Right.Evaluate(instance);
            if (Left.NodeType == ExpressionType.Identifier)
            {
                var exp = (NameExpression)Left;
                Metadata.Prototype proto = instance.GetPrototype();
                if (!proto.HasMember(exp.Name))
                {
                    proto.DeclareVariable(exp.Name, Right);
                }
                instance[exp.Name] = value;
            }
            else if (Left.NodeType == ExpressionType.Indexer)
            {
                var array = (IndexExpression)Left;
                array.SetArray(instance, value);
            }
            else if (Left.NodeType == ExpressionType.MemberAccess)
            {
                var exp = (MemberExpression)Left;
                var result = exp.Target.Evaluate(instance);
                Metadata.Prototype proto = result.GetPrototype();
                if (!proto.HasMember(exp.Name))
                {
                    proto.DeclareVariable(exp.Name, Right);
                }
                result[exp.Name] = value;
            }
            return value;
        }
#endif

        public override TResult Accept<TResult>(IExpressionVisitor<TResult> visitor)
        {
            return visitor.VisitAssignment(this);
        }

        public override void GenerateCode(MethodBodyGenerator generator)
        {
            //todo implementation pending
            if (Left.NodeType == ExpressionType.Identifier)
            {
                var exp = (NameExpression)Left;
                Binding binding = exp.Binding;
                if (binding.IsMember && generator.Method.IsStatic == false)
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
            }
        }

        public override string ToString()
        {
            return string.Concat(Left.ToString(), "=", Right.ToString());
        }
    }
}
