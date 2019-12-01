using FluidScript.Reflection.Emit;
using System;
using System.Linq;

namespace FluidScript.Compiler.SyntaxTree
{
    public class ArrayLiteralExpression : Expression
    {
        public readonly Expression[] Expressions;

        public TypeSyntax Type { get; }

        public ArrayLiteralExpression(Expression[] expressions, TypeSyntax type) : base(ExpressionType.Array)
        {
            Expressions = expressions;
            Type = type;
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
            Type type = ResultType(generator).GetElementType();
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

        protected override void ResolveType(MethodBodyGenerator member)
        {
            var typeName = Type.ToString();
            Type type = member.GetType(typeName);
            ResolvedType = type.MakeArrayType();
        }

        public override string ToString()
        {
            return string.Concat("[", string.Join(",", Expressions.Select(exp => exp.ToString())), "]");
        }
    }
}
