using FluidScript.Compiler.Emit;

namespace FluidScript.Compiler.SyntaxTree
{
    public class TernaryOperatorExpression : Expression
    {
        public readonly Expression First;

        public readonly Expression Second;

        public readonly Expression Third;

        public TernaryOperatorExpression(Expression first, Expression second, Expression third) : base(ExpressionType.Question)
        {
            First = first;
            Second = second;
            Third = third;
        }

#if Runtime
        public override RuntimeObject Evaluate(RuntimeObject instance)
        {
            return First.Evaluate(instance).ToBool() ? Second.Evaluate(instance) : Third.Evaluate(instance);
        }
#endif

        protected override void ResolveType(OptimizationInfo info)
        {
            var condition = First.PrimitiveType(info);
            if (condition == FluidScript.RuntimeType.Bool)
            {
                var a = Second.PrimitiveType(info);
                var b = Third.PrimitiveType(info);
                if (a == b)
                {
                    ResolvedPrimitiveType = a;
                    ResolvedType = Second.ReflectedType();
                }
                else
                {
                    if (a != FluidScript.RuntimeType.Any && b != FluidScript.RuntimeType.Any)
                    {
                        ResolvedPrimitiveType = a & b;
                        ResolvedType = TypeUtils.ToType(ResolvedPrimitiveType);
                    }
                    else
                    {
                        ResolvedPrimitiveType = FluidScript.RuntimeType.Any;
                        ResolvedType = typeof(object);
                    }
                }
            }
            else
            {
                throw new System.Exception("expected bool type");
            }
        }

        public override void GenerateCode(ILGenerator generator, MethodOptimizationInfo info)
        {
            var resultType = PrimitiveType(info);
            First.GenerateCode(generator, info);
            // Branch if the condition is false.
            var startOfElse = generator.CreateLabel();
            generator.BranchIfFalse(startOfElse);
            Second.GenerateCode(generator, info);
            var secondType = Second.PrimitiveType(info);
            EmitConvertion.Convert(generator, resultType, secondType, info);
        }
    }
}
