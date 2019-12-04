using FluidScript.Reflection.Emit;

namespace FluidScript.Compiler.SyntaxTree
{
    public class TernaryOperatorExpression : Expression
    {
        public readonly Expression First;

        public readonly Expression Second;

        public readonly Expression Third;

        private System.Reflection.MethodInfo implicitCall;

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

        protected override void ResolveType(MethodBodyGenerator generator)
        {
            var condition = First.ResultType(generator);
            if (condition == typeof(Boolean))
            {
                var firstType = Second.ResultType(generator);
                var secondType = Third.ResultType(generator);
                if (firstType == secondType)
                {
                    ResolvedType = secondType;
                    return;
                }
                if (TypeUtils.TryImplicitConvert(firstType, secondType, out System.Reflection.MethodInfo method))
                {
                    ResolvedType = secondType;
                    implicitCall = method;
                    return;
                }
                if (TypeUtils.TryImplicitConvert(secondType, firstType, out method))
                {
                    ResolvedType = firstType;
                    implicitCall = method;
                    return;
                }
            }
            else
            {
                throw new System.Exception("expected bool type");
            }
        }

        public override void GenerateCode(MethodBodyGenerator generator)
        {
            // Find the result type.
            var firstType = Second.ResultType(generator);
            var secondType = Third.ResultType(generator);
            First.GenerateCode(generator);
            // Branch if the condition is false.
            var startOfElse = generator.CreateLabel();
            generator.BranchIfFalse(startOfElse);
            Second.GenerateCode(generator);
            if (firstType != secondType && implicitCall == null && TypeUtils.TryImplicitConvert(firstType, secondType, out System.Reflection.MethodInfo method))
            {
                ResolvedType = secondType;
                implicitCall = method;
                generator.Call(implicitCall);
            }
            // Branch to the end.
            var end = generator.CreateLabel();
            generator.Branch(end);
            generator.DefineLabelPosition(startOfElse);

            Third.GenerateCode(generator);
            if (firstType != secondType && implicitCall == null && TypeUtils.TryImplicitConvert(secondType, firstType, out implicitCall))
            {
                ResolvedType = firstType;
                generator.Call(implicitCall);
            }

            generator.DefineLabelPosition(end);

        }
    }
}
