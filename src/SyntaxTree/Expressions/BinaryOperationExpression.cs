using System.Runtime.Serialization;

namespace FluidScript.SyntaxTree.Expressions
{
    [System.Serializable]
    [DataContract]
    public class BinaryOperationExpression : Expression
    {
        public readonly IExpression Left;
        public readonly IExpression Right;

        public BinaryOperationExpression(SerializationInfo info, StreamingContext context) : base(info, context)
        {

        }

        public BinaryOperationExpression(IExpression left, IExpression right, Operation opCode) : base(opCode)
        {
            Left = left;
            Right = right;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        public override string ToString()
        {
            return "Binary Expression";
        }

        public override TReturn Accept<TReturn>(INodeVisitor<TReturn> visitor)
        {
            return visitor.VisitBinaryOperation(this);
        }
    }
}
