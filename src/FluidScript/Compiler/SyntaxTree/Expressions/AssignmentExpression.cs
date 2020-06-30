using FluidScript.Compiler.Emit;
using FluidScript.Extensions;
using FluidScript.Utils;

namespace FluidScript.Compiler.SyntaxTree
{
    public sealed class AssignmentExpression : Expression
    {
        #region GenerateOption
        const MethodGenerateOption Option = MethodGenerateOption.Dupplicate | MethodGenerateOption.Assign;
        #endregion

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

        public override void GenerateCode(MethodBodyGenerator generator, MethodGenerateOption option)
        {
            //todo index implementation pending
            if (Left.NodeType == ExpressionType.Identifier)
            {
                var exp = (NameExpression)Left;
                var binder = exp.Binder;
                if (binder.CanEmitThis)
                    generator.LoadArgument(0);
                Right.GenerateCode(generator, Option);
                binder.GenerateSet(generator);
            }
            else if (Left.NodeType == ExpressionType.MemberAccess)
            {
                var exp = (MemberExpression)Left;
                exp.Target.GenerateCode(generator);
                Right.GenerateCode(generator, Option);
                exp.Binder.GenerateSet(generator);
            }
            else if (Left.NodeType == ExpressionType.Indexer)
            {
                var exp = (IndexExpression)Left;
                exp.Target.GenerateCode(generator);
                System.Type type = exp.Target.Type;
                if (type.IsArray)
                {
                    exp.Arguments.Iterate((arg) =>
                    {
                        arg.GenerateCode(generator);
                        generator.CallStatic(ReflectionHelpers.IntegerToInt32);
                    });
                    Right.GenerateCode(generator, MethodGenerateOption.Dupplicate);
                    System.Type elementType = type.GetElementType();
                    generator.StoreArrayElement(elementType);
                }
                else
                {

                    exp.Arguments.Iterate((arg) => arg.GenerateCode(generator));
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
