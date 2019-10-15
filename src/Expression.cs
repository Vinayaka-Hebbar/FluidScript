using System.Runtime.Serialization;

namespace FluidScript
{
    [System.Serializable]
    public abstract class Expression : IExpression, ISerializable
    {
        internal static readonly Expression Empty = new EmptyExpression();
        internal readonly Operation OpCode;

        public Operation Kind => OpCode;

        public Expression(Operation opCode)
        {
            OpCode = opCode;
        }

        protected Expression(SerializationInfo info, StreamingContext context)
        {
            OpCode = (Operation)info.GetInt32("kind");
        }

        public abstract TReturn Accept<TReturn>(INodeVisitor<TReturn> visitor) where TReturn : IRuntimeObject;

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("kind", OpCode);
        }

        public enum Operation : byte
        {
            Unknown = 0,
            Literal = 1,
            Numeric = 2,
            Octal = 3,
            Hex = 4,
            Unicode = 5,
            String = 6,
            Bool = 7,
            Variable = 8,
            Constant = 9,
            Identifier = 10,
            Parenthesized = 11,
            Invocation = 12,
            AnonymousMethod = 13,
            Declaration = 14,
            //Known types
            Block = 15,
            Function = 16,
            Argument = 17,
            Comma = 24,
            //?
            Question = 27,
            PropertyAccess = 29,
            QualifiedNamespace = 30,
            New = 31,
            Out = 32,
            In = 33,
            This = 34,
            #region Math
            Plus = 66,
            Minus = 67,
            Multiply = 68,
            Divide = 69,
            Percent = 70,
            //^
            Circumflex = 71,
            #endregion
            #region Logical & Shift
            Equal = 127,
            // <
            Less = 128,
            // > 
            Greater = 129,
            // <=
            LessEqual = 130,
            // >= 
            GreaterEqual = 131,
            // == 
            EqualEqual = 132,
            //!
            Bang = 133,
            // != 
            BangEqual = 134,
            // << 
            LessLess = 135,
            // >> 
            GreaterGreater = 136,
            //|
            Or = 137,
            //||
            OrOr = 138,
            //&
            And = 139,
            //&&
            AndAnd = 140,
            #endregion
            #region PostFix Prefix
            PostfixPlusPlus = 143,
            PostfixMinusMinus = 144,
            PrefixPlusPlus = 145,
            PrefixMinusMinus = 146,
            #endregion
        }

    }

    internal sealed class EmptyExpression : Expression
    {
        public EmptyExpression() : base(Operation.Unknown)
        {
        }

        public override TReturn Accept<TReturn>(INodeVisitor<TReturn> visitor)
        {
            return default(TReturn);
        }
    }
}
