using FluidScript.Reflection.Emit;
using System;
using System.Linq;

namespace FluidScript.Compiler.SyntaxTree
{
    public class ArrayLiteralExpression : Expression
    {
        public readonly Expression[] Expressions;

        public TypeSyntax ArrayType { get; }

        public ArrayLiteralExpression(Expression[] expressions, TypeSyntax type) : base(ExpressionType.Array)
        {
            Expressions = expressions;
            ArrayType = type;
        }

#if Runtime
        public override RuntimeObject Evaluate(RuntimeObject instance)
        {
            RuntimeObject[] array = new RuntimeObject[Expressions.Length];
            for (int i = 0; i < Expressions.Length; i++)
            {
                array[i] = Expressions[i].Evaluate(instance);

            }
            return new Library.ArrayObject(array, FluidScript.RuntimeType.Any);
        }
#endif

        public override void GenerateCode(MethodBodyGenerator generator)
        {
            generator.LoadInt32(Expressions.Length);
            Type type = Type.GetElementType();
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
                    expression.GenerateCode(generator);
                    //todo box
                    //EmitConvertion.ToAny(generator, type);
                }
                generator.StoreArrayElement(type);
            }
        }

        public override TResult Accept<TResult>(IExpressionVisitor<TResult> visitor)
        {
            return visitor.VisitArrayLiteral(this);
        }

        public override string ToString()
        {
            return string.Concat("[", string.Join(",", Expressions.Select(exp => exp.ToString())), "]");
        }
    }
}
