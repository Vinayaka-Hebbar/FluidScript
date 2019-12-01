using System.Runtime.InteropServices;
using FluidScript.Reflection.Emit;

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
                var array = (InvocationExpression)Left;
                array.SetArray(instance, value);
            }else if(Left.NodeType == ExpressionType.MemberAccess)
            {
                var exp= (QualifiedExpression)Left;
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

        public override void GenerateCode(MethodBodyGenerator generator)
        {
            var resultType = Right.ResultType(generator);
            if(Right.NodeType == ExpressionType.Identifier)
            {
                var exp = (NameExpression)Left;

            }
            base.GenerateCode(generator);
        }

        public override string ToString()
        {
            return string.Concat(Left.ToString(), "=", Right.ToString());
        }
    }
}
