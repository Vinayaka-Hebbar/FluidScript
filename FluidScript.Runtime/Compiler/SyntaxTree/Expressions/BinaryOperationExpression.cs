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
            switch (NodeType)
            {
                case ExpressionType.Plus:
                    return Left.Evaluate() + Right.Evaluate();
                case ExpressionType.Minus:
                    return Left.Evaluate() - Right.Evaluate();
                case ExpressionType.Multiply:
                    return Left.Evaluate() * Right.Evaluate();
                case ExpressionType.Divide:
                    return Left.Evaluate() / Right.Evaluate();
                case ExpressionType.Percent:
                    return Left.Evaluate() % Right.Evaluate();
                case ExpressionType.Circumflex:
                    return Left.Evaluate() ^ Right.Evaluate();
                case ExpressionType.EqualEqual:
                    return Left.Evaluate() == Right.Evaluate();
                case ExpressionType.BangEqual:
                    return Left.Evaluate() != Right.Evaluate();
                case ExpressionType.Less:
                    return Left.Evaluate() < Right.Evaluate();
                case ExpressionType.LessEqual:
                    return Left.Evaluate() <= Right.Evaluate();
                case ExpressionType.LessLess:
                    return Left.Evaluate() << Right.Evaluate().ToInt32();
                case ExpressionType.Greater:
                    return Left.Evaluate() > Right.Evaluate();
                case ExpressionType.GreaterEqual:
                    return Left.Evaluate() >= Right.Evaluate();
                case ExpressionType.GreaterGreater:
                    return Left.Evaluate() >> (int)Right.Evaluate();
                case ExpressionType.And:
                    return Left.Evaluate() & Right.Evaluate();
                case ExpressionType.AndAnd:
                    return new Core.PrimitiveObject(Left.Evaluate().ToBool() && Right.Evaluate().ToBool());
                case ExpressionType.Or:
                    return Left.Evaluate() | Right.Evaluate();
                case ExpressionType.OrOr:
                    return new Core.PrimitiveObject(Left.Evaluate().ToBool() || Right.Evaluate().ToBool());
                case ExpressionType.Equal:
                    var value = Right.Evaluate();
                    if (Left.NodeType == ExpressionType.Identifier)
                    {
                        var exp = (NameExpression)Left;
                        Metadata.Prototype scope = exp.Prototype;
                        Reflection.DeclaredVariable variable = null;
                        if (scope.HasVariable(exp.Name))
                        {
                            variable = scope.GetVariable(exp.Name);
                        }
                        else
                        {
                            variable = scope.DeclareVariable(exp.Name, Right);
                        }
                        if (variable != null)
                        {
                            variable.Value = value;
                        }
                    }
                    else if (Left.NodeType == ExpressionType.Indexer)
                    {
                        var array = (InvocationExpression)Left;
                        array.SetArray(value);
                    }
                    return value;
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
            if (leftType != FluidScript.RuntimeType.Any && rightType != FluidScript.RuntimeType.Any)
            {
                switch (NodeType)
                {
                    case ExpressionType.Plus:
                        if (TypeUtils.CheckType(leftType, FluidScript.RuntimeType.String) || TypeUtils.CheckType(rightType, FluidScript.RuntimeType.String))
                        {
                            ResolvedPrimitiveType = FluidScript.RuntimeType.String;
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
                        ResolvedPrimitiveType = FluidScript.RuntimeType.Bool;
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

        public override string ToString()
        {
            var operation = string.Empty;
            switch (NodeType)
            {
                case ExpressionType.Plus:
                    operation = "+";
                    break;
                case ExpressionType.Minus:
                    operation = "-";
                    break;
                case ExpressionType.Multiply:
                    operation = "*";
                    break;
                case ExpressionType.Divide:
                    operation = "/";
                    break;
                case ExpressionType.Percent:
                    operation = "%";
                    break;
                case ExpressionType.Circumflex:
                    operation = "^";
                    break;
                case ExpressionType.EqualEqual:
                    operation = "==";
                    break;
                case ExpressionType.BangEqual:
                    operation = "!=";
                    break;
                case ExpressionType.Less:
                    operation = "<";
                    break;
                case ExpressionType.LessEqual:
                    operation = "<=";
                    break;
                case ExpressionType.LessLess:
                    operation = "<<";
                    break;
                case ExpressionType.Greater:
                    operation = ">";
                    break;
                case ExpressionType.GreaterEqual:
                    operation = ">=";
                    break;
                case ExpressionType.GreaterGreater:
                    operation = ">>";
                    break;
                case ExpressionType.And:
                    operation = "&";
                    break;
                case ExpressionType.AndAnd:
                    operation = "&&";
                    break;
                case ExpressionType.Or:
                    operation = "|";
                    break;
                case ExpressionType.OrOr:
                    operation = "||";
                    break;
                case ExpressionType.Equal:
                    operation = "=";
                    break;
            }
            return string.Concat(Left.ToString(), operation, Right.ToString());
        }
    }
}
