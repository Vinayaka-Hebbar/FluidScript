using FluidScript.Compiler.Metadata;
using FluidScript.Reflection.Emit;
using System.Collections.Generic;

namespace FluidScript.Compiler.SyntaxTree
{
    public class BinaryOperationExpression : Expression
    {
        public readonly Expression Left;
        public readonly Expression Right;

        public BinaryOperationExpression(Expression left, Expression right, ExpressionType opCode) : base(opCode)
        {
            Left = left;
            Right = right;
        }

        public override IEnumerable<Node> ChildNodes() => Childs(Left, Right);

#if Runtime
        public override RuntimeObject Evaluate(RuntimeObject instance)
        {
            RuntimeObject left = Left.Evaluate(instance);
            switch (NodeType)
            {
                case ExpressionType.Plus:
                    return left + Right.Evaluate(instance);
                case ExpressionType.Minus:
                    return left - Right.Evaluate(instance);
                case ExpressionType.Multiply:
                    return left * Right.Evaluate(instance);
                case ExpressionType.Divide:
                    return left / Right.Evaluate(instance);
                case ExpressionType.Percent:
                    return left % Right.Evaluate(instance);
                case ExpressionType.Circumflex:
                    return left ^ Right.Evaluate(instance);
                case ExpressionType.EqualEqual:
                    return left == Right.Evaluate(instance);
                case ExpressionType.BangEqual:
                    return left != Right.Evaluate(instance);
                case ExpressionType.Less:
                    return left < Right.Evaluate(instance);
                case ExpressionType.LessEqual:
                    return left <= Right.Evaluate(instance);
                case ExpressionType.LessLess:
                    return left << Right.Evaluate(instance).ToInt32();
                case ExpressionType.Greater:
                    return left > Right.Evaluate(instance);
                case ExpressionType.GreaterEqual:
                    return left >= Right.Evaluate(instance);
                case ExpressionType.GreaterGreater:
                    return left >> (int)Right.Evaluate(instance);
                case ExpressionType.And:
                    return left & Right.Evaluate(instance);
                case ExpressionType.AndAnd:
                    return new Library.PrimitiveObject(left.ToBool() && Right.Evaluate(instance).ToBool());
                case ExpressionType.Or:
                    return left | Right.Evaluate(instance);
                case ExpressionType.OrOr:
                    return new Library.PrimitiveObject(left.ToBool() || Right.Evaluate(instance).ToBool());
            }
            return RuntimeObject.Null;
        }

        public override RuntimeObject Evaluate(Prototype proto)
        {
            RuntimeObject left = Left.Evaluate(proto);
            switch (NodeType)
            {
                case ExpressionType.Plus:
                    return left + Right.Evaluate(proto);
                case ExpressionType.Minus:
                    return left - Right.Evaluate(proto);
                case ExpressionType.Multiply:
                    return left * Right.Evaluate(proto);
                case ExpressionType.Divide:
                    return left / Right.Evaluate(proto);
                case ExpressionType.Percent:
                    return left % Right.Evaluate(proto);
                case ExpressionType.Circumflex:
                    return left ^ Right.Evaluate(proto);
                case ExpressionType.EqualEqual:
                    return left == Right.Evaluate(proto);
                case ExpressionType.BangEqual:
                    return left != Right.Evaluate(proto);
                case ExpressionType.Less:
                    return left < Right.Evaluate(proto);
                case ExpressionType.LessEqual:
                    return left <= Right.Evaluate(proto);
                case ExpressionType.LessLess:
                    return left << Right.Evaluate(proto).ToInt32();
                case ExpressionType.Greater:
                    return left > Right.Evaluate(proto);
                case ExpressionType.GreaterEqual:
                    return left >= Right.Evaluate(proto);
                case ExpressionType.GreaterGreater:
                    return left >> (int)Right.Evaluate(proto);
                case ExpressionType.And:
                    return left & Right.Evaluate(proto);
                case ExpressionType.AndAnd:
                    return new Library.PrimitiveObject(left.ToBool() && Right.Evaluate(proto).ToBool());
                case ExpressionType.Or:
                    return left | Right.Evaluate(proto);
                case ExpressionType.OrOr:
                    return new Library.PrimitiveObject(left.ToBool() || Right.Evaluate(proto).ToBool());
            }
            return base.Evaluate(proto);
        }
#endif

        protected override void ResolveType(MethodBodyGenerator generator)
        {
            //todo resolve type for operator overload
            var leftType = Left.GetRuntimeType(generator);
            var rightType = Right.GetRuntimeType(generator);
            if (leftType != RuntimeType.Any && rightType != RuntimeType.Any)
            {
                switch (NodeType)
                {
                    case ExpressionType.Plus:
                        if (TypeUtils.CheckType(leftType, RuntimeType.String) || TypeUtils.CheckType(rightType, RuntimeType.String))
                        {
                            ResolvedType = typeof(string);
                            return;
                        }
                        ResolvedRuntimeType = leftType & rightType;
                        break;
                    case ExpressionType.Minus:
                    case ExpressionType.Multiply:
                    case ExpressionType.Divide:
                        ResolvedRuntimeType = leftType & rightType;
                        break;
                    case ExpressionType.BangEqual:
                    case ExpressionType.EqualEqual:
                    case ExpressionType.Less:
                    case ExpressionType.LessEqual:
                    case ExpressionType.Greater:
                    case ExpressionType.GreaterEqual:
                        ResolvedRuntimeType = RuntimeType.Bool;
                        break;
                }

            }
        }

        public override void GenerateCode(MethodBodyGenerator generator)
        {
            switch (NodeType)
            {
                case ExpressionType.Plus:
                    var resultType = GetRuntimeType(generator);
                    if (resultType == RuntimeType.String)
                    {
                        GenerateStringAdd(generator);
                        return;
                    }
                    LoadValues(generator);
                    generator.Add();
                    break;
                case ExpressionType.Minus:
                    LoadValues(generator);
                    generator.Subtract();
                    break;
                case ExpressionType.Multiply:
                    LoadValues(generator);
                    generator.Multiply();
                    break;
                case ExpressionType.Divide:
                    LoadValues(generator);
                    generator.Divide();
                    break;
                case ExpressionType.EqualEqual:
                    LoadValues(generator);
                    generator.CompareEqual();
                    break;
                case ExpressionType.BangEqual:
                    LoadValues(generator);
                    generator.CompareEqual();
                    generator.LoadInt32(0);
                    generator.CompareEqual();
                    break;
                case ExpressionType.Less:
                    LoadValues(generator);
                    generator.CompareLessThan();
                    break;
                case ExpressionType.LessEqual:
                    LoadValues(generator);
                    if (Left.GetRuntimeType(generator) == RuntimeType.Double || Right.GetRuntimeType(generator) == RuntimeType.Double)
                        generator.CompareGreaterThanUnsigned();
                    else
                        generator.CompareGreaterThan();
                    generator.LoadInt32(0);
                    generator.CompareEqual();
                    break;
                case ExpressionType.Greater:
                    LoadValues(generator);
                    generator.CompareGreaterThan();
                    break;
                case ExpressionType.GreaterEqual:
                    LoadValues(generator);
                    if (Left.GetRuntimeType(generator) == RuntimeType.Double || Right.GetRuntimeType(generator) == RuntimeType.Double)
                        generator.CompareLessThanUnsigned();
                    else
                        generator.CompareLessThan();
                    generator.LoadInt32(0);
                    generator.CompareEqual();
                    break;

            }
        }

        private void LoadValues(MethodBodyGenerator generator)
        {
            var leftType = Left.GetRuntimeType(generator);
            var rightType = Right.GetRuntimeType(generator);
            var expectedType = leftType & rightType;
            Left.GenerateCode(generator);
            if (leftType == RuntimeType.Char && rightType == RuntimeType.Char)
            {
                generator.ConvertToInt32();
                Right.GenerateCode(generator);
                generator.ConvertToInt32();
                return;
            }
            if (leftType != expectedType)
                EmitConvertion.ToPrimitive(generator, expectedType);
            Right.GenerateCode(generator);
            if (rightType != expectedType)
                EmitConvertion.ToPrimitive(generator, expectedType);
        }

        private void GenerateStringAdd(MethodBodyGenerator generator)
        {
            var leftType = Left.GetRuntimeType(generator);
            var rightType = Right.GetRuntimeType(generator);
            ResolvedType = typeof(string);
            //todo result optimization
            //load the left into stack
            Left.GenerateCode(generator);
            //todo if need to be primitive
            //box if int ,bool or other object
            EmitConvertion.ToString(generator, leftType, Left.ToString());
            Right.GenerateCode(generator);
            EmitConvertion.ToString(generator, rightType, Right.ToString());
            //check both are string
            if (leftType == rightType)
            {
                generator.Call(ReflectionHelpers.StringConcat_Two_String);
                return;
            }
            generator.Call(ReflectionHelpers.StringConcat_Two_Object);
        }

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
            }
            return string.Concat(Left.ToString(), operation, Right.ToString());
        }
    }
}
