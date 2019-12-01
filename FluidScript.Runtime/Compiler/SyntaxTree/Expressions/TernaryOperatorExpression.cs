using FluidScript.Reflection.Emit;

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

        protected override void ResolveType(MethodBodyGenerator generator)
        {
            var condition = First.GetRuntimeType(generator);
            if (condition == RuntimeType.Bool)
            {
                var a = Second.GetRuntimeType(generator);
                var b = Third.GetRuntimeType(generator);
                if (a == b)
                {
                    ResolvedType = Second.ResultType(generator);
                }
                else
                {
                    if (a != RuntimeType.Any && b != RuntimeType.Any)
                    {
                        ResolvedRuntimeType = a & b;
                    }
                    else
                    {
                        ResolvedRuntimeType = RuntimeType.Any;
                    }
                }
            }
            else
            {
                throw new System.Exception("expected bool type");
            }
        }

        public override void GenerateCode(MethodBodyGenerator generator)
        {
            // Calculate the result type.
            var resultType = GetRuntimeType(generator);
            First.GenerateCode(generator);
            // Branch if the condition is false.
            var startOfElse = generator.CreateLabel();
            generator.BranchIfFalse(startOfElse);
            Second.GenerateCode(generator);
            var secondType = Second.GetRuntimeType(generator);
            EmitConvertion.Convert(generator, resultType, secondType);
            // Branch to the end.
            var end = generator.CreateLabel();
            generator.Branch(end);
            generator.DefineLabelPosition(startOfElse);

            Third.GenerateCode(generator);
            var thirdType = Second.GetRuntimeType(generator);
            EmitConvertion.Convert(generator, resultType, thirdType);

            generator.DefineLabelPosition(end);

        }
    }
}
