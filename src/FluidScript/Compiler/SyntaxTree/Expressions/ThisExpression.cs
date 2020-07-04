using FluidScript.Compiler.Emit;

namespace FluidScript.Compiler.SyntaxTree
{
    public class ThisExpression : Expression
    {
        public ThisExpression() : base(ExpressionType.This)
        {
        }

        public override TResult Accept<TResult>(IExpressionVisitor<TResult> visitor)
        {
            return visitor.VisitThis(this);
        }

        public override void GenerateCode(MethodBodyGenerator generator, MethodCompileOption options)
        {
            if (generator.Method is Generators.DynamicMethodGenerator)
            {
                // for annonymous type
                // if this is used in anonymous function or objects
                var variable = generator.GetLocalVariable("__value");
                generator.LoadVariable(variable);
            }
            else
            {
                generator.LoadArgument(0);
            }
        }

        public override string ToString()
        {
            return "this";
        }
    }
}
