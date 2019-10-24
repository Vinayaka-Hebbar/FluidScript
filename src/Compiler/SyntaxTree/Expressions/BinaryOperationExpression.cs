using FluidScript.Compiler.Emit;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace FluidScript.Compiler.SyntaxTree
{
    [System.Serializable]
    [DataContract]
    public class BinaryOperationExpression : Expression
    {
        public readonly Expression Left;
        public readonly Expression Right;

        public BinaryOperationExpression(Expression left, Expression right, NodeType opCode) : base(opCode)
        {
            Left = left;
            Right = right;
        }

        public override IEnumerable<Node> ChildNodes => Childs(Left, Right);

        public override TReturn Accept<TReturn>(INodeVisitor<TReturn> visitor)
        {
            return visitor.VisitBinaryOperation(this);
        }

        public override void GenerateCode(ILGenerator generator, OptimizationInfo info)
        {
            switch (NodeType)
            {
                case NodeType.Plus:
                    GenerateAdd(generator, info);
                    break;
            }
        }

        private void GenerateAdd(ILGenerator generator, OptimizationInfo info)
        {
            var leftType = Left.ResultType;
            var rightType = Right.ResultType;
            //if any of the values is string concat it
            if (TypeUtils.CheckType(leftType, ObjectType.String) || TypeUtils.CheckType(rightType, ObjectType.String))
            {
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
            if (leftType != ObjectType.Object && rightType != ObjectType.Object)
            {
                var resultType = leftType & rightType;
                Left.GenerateCode(generator, info);
                if (leftType == ObjectType.Char && rightType == ObjectType.Char)
                {
                    generator.ConvertToInt32();
                    Right.GenerateCode(generator, info);
                    generator.ConvertToInt32();
                    generator.Add();
                    return;
                }
                if (leftType != resultType)
                    EmitConvertion.ToNumber(generator, resultType);
                Right.GenerateCode(generator, info);
                if (rightType != resultType)
                    EmitConvertion.ToNumber(generator, rightType);
                generator.Add();
            }
            else
            {
                //Handle custom convertion
            }
        }

    }
}
