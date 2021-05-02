using FluidScript.Runtime;

namespace FluidScript.Compiler.SyntaxTree
{
    public sealed class NullPropegatorExpression : Expression
    {
        public readonly Expression Left;
        public readonly Expression Right;

        public System.Reflection.MethodInfo Conversion
        {
            get;
            set;
        }

        public NullPropegatorExpression(Expression left, Expression right) : base(ExpressionType.Invocation)
        {
            Left = left;
            Right = right;
        }

        public override TResult Accept<TResult>(IExpressionVisitor<TResult> visitor)
        {
            return visitor.VisitNullPropegator(this);
        }

        public override string ToString()
        {
            return string.Concat(Left.ToString(), "??", Right.ToString());
        }

        public override void GenerateCode(Emit.MethodBodyGenerator generator, Emit.MethodCompileOption option)
        {
            Left.GenerateCode(generator, option);
            generator.Duplicate();
            var end = generator.CreateLabel();
            if (Left.Type.IsValueType)
            {
                if (Left.Type.TryImplicitConvert(TypeProvider.BooleanType, out System.Reflection.MethodInfo op_Implicit))
                {
                    generator.CallStatic(op_Implicit);
                    generator.CallStatic(Utils.ReflectionHelpers.BoooleanToBool);
                }
                else
                {
                    throw new System.InvalidCastException($"Unable to cast object of type {Left.Type} to {TypeProvider.BooleanType}");
                }
            }
            generator.BranchIfTrue(end);
            generator.Pop();
            Right.GenerateCode(generator, Emit.MethodCompileOption.Dupplicate);
            if (Conversion != null)
            {
                if (Right.Type.IsValueType && Conversion.GetParameters()[0].ParameterType.IsValueType == false)
                    generator.Box(Right.Type);
                generator.CallStatic(Conversion);
            }

            generator.DefineLabelPosition(end);
        }
    }
}
