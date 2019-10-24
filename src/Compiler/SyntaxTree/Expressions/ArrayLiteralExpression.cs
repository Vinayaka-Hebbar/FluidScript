using FluidScript.Compiler.Emit;
using System;

namespace FluidScript.Compiler.SyntaxTree
{
    public class ArrayLiteralExpression : Expression
    {
        public readonly Expression[] Expressions;

        public readonly string TypeName;

        public ArrayLiteralExpression(Expression[] expressions, string typeName) : base(NodeType.Array)
        {
            Expressions = expressions;
            TypeName = typeName;
        }

        public override TReturn Accept<TReturn>(INodeVisitor<TReturn> visitor)
        {
            throw new NotImplementedException();
        }

        public override void GenerateCode(ILGenerator generator, OptimizationInfo info)
        {
            generator.LoadInt32(Expressions.Length);
            Type type = info.ToType(TypeName);
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
