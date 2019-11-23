using FluidScript.Compiler.Emit;
using System;
using System.Linq;

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
            ResolvedPrimitiveType = FluidScript.RuntimeType.Array;
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


        public override void GenerateCode(ILGenerator generator, MethodOptimizationInfo info)
        {
            generator.LoadInt32(Expressions.Length);
            Type type = ResultType(info).GetElementType();
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

        protected override void ResolveType(OptimizationInfo info)
        {
            ResolvedPrimitiveType |= TypeUtils.From(TypeName.FullName).Enum;
            Type type = info.GetType(TypeName);
            ResolvedType = type.MakeArrayType();
        }

        public override string ToString()
        {
            return string.Concat("[", string.Join(",", Expressions.Select(exp => exp.ToString())), "]");
        }
    }
}
