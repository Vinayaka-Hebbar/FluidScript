using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluidScript.Compiler.SyntaxTree
{
    public class SuperExpression : Expression
    {
        public SuperExpression() : base(ExpressionType.Super)
        {
        }

        public override TResult Accept<TResult>(IExpressionVisitor<TResult> visitor)
        {
            return visitor.VisitSuper(this);
        }

        public override void GenerateCode(Emit.MethodBodyGenerator generator, Emit.MethodGenerateOption options)
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
            return "super";
        }
    }
}
