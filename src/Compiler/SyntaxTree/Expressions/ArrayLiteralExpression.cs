using FluidScript.Compiler.Emit;
using System;

namespace FluidScript.Compiler.SyntaxTree
{
    public class ArrayLiteralExpression : Expression
    {
        public readonly Expression[] Expressions;

        public override Emit.TypeName TypeName { get; }

        public ArrayLiteralExpression(Expression[] expressions, Emit.TypeName typeName) : base(ExpressionType.Array)
        {
            Expressions = expressions;
            TypeName = typeName;
            ResolvedPrimitiveType = FluidScript.PrimitiveType.Array;
        }

        public override PrimitiveType PrimitiveType()
        {
            if(ResolvedPrimitiveType == FluidScript.PrimitiveType.Array)
            {
                ResolvedPrimitiveType |= TypeUtils.From(TypeName.FullName).Enum;
            }
            return ResolvedPrimitiveType;
        }

        public override void GenerateCode(ILGenerator generator, MethodOptimizationInfo info)
        {
            generator.LoadInt32(Expressions.Length);
            Type type = info.GetType(TypeName);
            ResolvedType = type.MakeArrayType();
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
                    //todo box
                    //EmitConvertion.ToAny(generator, type);
                }
                generator.StoreArrayElement(type);
            }
        }
    }
}
