using FluidScript.Compiler.Emit;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace FluidScript.Compiler.SyntaxTree
{
    [DataContract]
    public class BinaryOperationExpression : Expression
    {
        public readonly Expression Left;
        public readonly Expression Right;

        public BinaryOperationExpression(Expression left, Expression right, ExpressionType opCode) : base(opCode)
        {
            Left = left;
            Right = right;
        }

        public override IEnumerable<Node> ChildNodes => Childs(Left, Right);

        public override RuntimeObject Evaluate()
        {
            var left = Left.Evaluate();
            var right = Right.Evaluate();
            switch (NodeType)
            {
                case ExpressionType.Plus:
                    return left + right;
            }
            return base.Evaluate();
        }

        /// <summary>
        /// Todo remove arguments by 
        /// </summary>
        /// <param name="info"></param>
        protected override void ResolveType(OptimizationInfo info)
        {
            var leftType = Left.PrimitiveType(info);
            var rightType = Right.PrimitiveType(info);
            if (leftType != FluidScript.PrimitiveType.Any && rightType != FluidScript.PrimitiveType.Any)
            {
                switch (NodeType)
                {
                    case ExpressionType.Plus:
                        if (TypeUtils.CheckType(leftType, FluidScript.PrimitiveType.String) || TypeUtils.CheckType(rightType, FluidScript.PrimitiveType.String))
                        {
                            ResolvedPrimitiveType = FluidScript.PrimitiveType.String;
                            ResolvedType = typeof(string);
                            return;
                        }
                        ResolvedPrimitiveType = leftType & rightType;
                        ResolvedType = Emit.TypeUtils.ToType(ResolvedPrimitiveType);
                        break;
                    case ExpressionType.Minus:
                    case ExpressionType.Multiply:
                    case ExpressionType.Divide:
                        ResolvedPrimitiveType = leftType & rightType;
                        ResolvedType = Emit.TypeUtils.ToType(ResolvedPrimitiveType);
                        break;
                    case ExpressionType.BangEqual:
                    case ExpressionType.EqualEqual:
                    case ExpressionType.Less:
                    case ExpressionType.LessEqual:
                    case ExpressionType.Greater:
                    case ExpressionType.GreaterEqual:
                        ResolvedPrimitiveType = FluidScript.PrimitiveType.Bool;
                        ResolvedType = typeof(bool);
                        break;
                }

            }
        }

#if Emit
        public override void GenerateCode(ILGenerator generator, MethodOptimizationInfo info)
        {
            switch (NodeType)
            {
                case ExpressionType.Plus:
                    var resultType = PrimitiveType(info);
                    if (resultType == FluidScript.PrimitiveType.String)
                    {
                        GenerateStringAdd(generator, info);
                        return;
                    }
                    LoadValues(generator, info);
                    generator.Add();
                    break;
                case ExpressionType.Minus:
                    LoadValues(generator, info);
                    generator.Subtract();
                    break;
                case ExpressionType.Multiply:
                    LoadValues(generator, info);
                    generator.Multiply();
                    break;
                case ExpressionType.Divide:
                    LoadValues(generator, info);
                    generator.Divide();
                    break;
                case ExpressionType.EqualEqual:
                    LoadValues(generator, info);
                    generator.CompareEqual();
                    break;
                case ExpressionType.BangEqual:
                    LoadValues(generator, info);
                    generator.CompareEqual();
                    generator.LoadInt32(0);
                    generator.CompareEqual();
                    break;
                case ExpressionType.Less:
                    LoadValues(generator, info);
                    generator.CompareLessThan();
                    break;
                case ExpressionType.LessEqual:
                    LoadValues(generator, info);
                    if (Left.PrimitiveType(info) == FluidScript.PrimitiveType.Double || Right.PrimitiveType(info) == FluidScript.PrimitiveType.Double)
                        generator.CompareGreaterThanUnsigned();
                    else
                        generator.CompareGreaterThan();
                    generator.LoadInt32(0);
                    generator.CompareEqual();
                    break;
                case ExpressionType.Greater:
                    LoadValues(generator, info);
                    generator.CompareGreaterThan();
                    break;
                case ExpressionType.GreaterEqual:
                    LoadValues(generator, info);
                    if (Left.PrimitiveType(info) == FluidScript.PrimitiveType.Double || Right.PrimitiveType(info) == FluidScript.PrimitiveType.Double)
                        generator.CompareLessThanUnsigned();
                    else
                        generator.CompareLessThan();
                    generator.LoadInt32(0);
                    generator.CompareEqual();
                    break;

            }
        }



        private void LoadValues(ILGenerator generator, MethodOptimizationInfo info)
        {
            var leftType = Left.PrimitiveType(info);
            var rightType = Right.PrimitiveType(info);
            var expectedType = leftType & rightType;
            Left.GenerateCode(generator, info);
            if (leftType == FluidScript.PrimitiveType.Char && rightType == FluidScript.PrimitiveType.Char)
            {
                generator.ConvertToInt32();
                Right.GenerateCode(generator, info);
                generator.ConvertToInt32();
                return;
            }
            if (leftType != expectedType)
                EmitConvertion.ToPrimitive(generator, expectedType);
            Right.GenerateCode(generator, info);
            if (rightType != expectedType)
                EmitConvertion.ToPrimitive(generator, expectedType);
        }

        private void GenerateStringAdd(ILGenerator generator, MethodOptimizationInfo info)
        {
            var leftType = Left.PrimitiveType(info);
            var rightType = Right.PrimitiveType(info);
            ResolvedType = typeof(string);
            //todo result optimization
            //load the left into stack
            Left.GenerateCode(generator, info);
            //todo if need to be primitive
            //box if int ,bool or other object
            EmitConvertion.ToString(generator, leftType, Left.ToString());
            Right.GenerateCode(generator, info);
            EmitConvertion.ToString(generator, rightType, Right.ToString());
            //check both are string
            if (leftType == rightType)
            {
                generator.Call(ReflectionHelpers.StringConcat_Two_String);
                return;
            }
            generator.Call(ReflectionHelpers.StringConcat_Two_Object);
        }

        public override Core.RuntimeObject Evaluate()
        {
            var left = Left.Evaluate();
            var right = Right.Evaluate();
            switch (NodeType)
            {
                case ExpressionType.Plus:
                    return left + right;
                case ExpressionType.Minus:
                    return left - right;
                case ExpressionType.Multiply:
                    return left * right;
                case ExpressionType.Divide:
                    return left / right;
            }
            return Core.RuntimeObject.NaN;
        }
#endif
    }
}
