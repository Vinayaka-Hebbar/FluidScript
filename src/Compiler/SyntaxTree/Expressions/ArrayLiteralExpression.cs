using FluidScript.Compiler.Emit;
using System;

namespace FluidScript.Compiler.SyntaxTree
{
    public class ArrayLiteralExpression : Expression
    {
        public readonly Expression[] Expressions;

        public override string TypeName { get; }

        public ArrayLiteralExpression(Expression[] expressions, string typeName) : base(ExpressionType.Array)
        {
            Expressions = expressions;
            TypeName = typeName;
        }

        public override void GenerateCode(ILGenerator generator, OptimizationInfo info)
        {
            generator.LoadInt32(Expressions.Length);
            Type type = info.TypeProvider.GetType(TypeName);
            generator.NewArray(type);
            for (int i = 0; i < Expressions.Length; i++)
            {
                generator.Duplicate();
                generator.LoadInt32(i);
                var expression = Expressions[i];
                if (expression == null)
                    generator.LoadNull();
                else
                {
                    expression.GenerateCode(generator, info);
                    EmitConvertion.ToAny(generator, type);
                }
                generator.StoreArrayElement(type);
            }
        }
    }
}
