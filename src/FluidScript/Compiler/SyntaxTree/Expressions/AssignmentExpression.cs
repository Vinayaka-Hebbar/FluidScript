using FluidScript.Compiler.Emit;
using FluidScript.Extensions;
using FluidScript.Utils;

namespace FluidScript.Compiler.SyntaxTree
{
    public sealed class AssignmentExpression : Expression
    {
        #region GenerateOption
        const MethodCompileOption Option = MethodCompileOption.Dupplicate | MethodCompileOption.Return;
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

        public override void GenerateCode(MethodBodyGenerator generator, MethodCompileOption option)
        {
            //todo index implementation pending
            if (Left.NodeType == ExpressionType.Identifier)
            {
                var exp = (NameExpression)Left;
                var binder = exp.Binder;
                // binder is null create new local variable
                if(binder is null)
                    exp.Binder = binder = new Binders.VariableBinder(generator.DeclareVariable(Right.Type, exp.Name));
                if ((binder.Attributes & Binders.BindingAttributes.HasThis) != 0)
                    generator.LoadArgument(0);
                Right.GenerateCode(generator, Option);
                binder.GenerateSet(Right, generator, option);
            }
            else if (Left.NodeType == ExpressionType.MemberAccess)
            {
                var exp = (MemberExpression)Left;
                exp.Target.GenerateCode(generator);
                // member assign for dynamic to be Any
                if ((exp.Binder.Attributes & Binders.BindingAttributes.Dynamic) == Binders.BindingAttributes.Dynamic)
                    generator.Box(TypeProvider.AnyType);
                Right.GenerateCode(generator, Option);
                exp.Binder.GenerateSet(Right, generator, option);
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
                        if (arg.Type == typeof(Integer))
                            generator.CallStatic(ReflectionHelpers.IntegerToInt32);
                    });
                    Right.GenerateCode(generator, MethodCompileOption.Dupplicate);
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
