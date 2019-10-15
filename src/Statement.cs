namespace FluidScript
{
    public abstract class Statement
    {
        internal static readonly Statement Empty = new EmptyStatement();

        public readonly Operation OpCode;

        protected Statement(Operation opCode)
        {
            OpCode = opCode;
        }

        public abstract TReturn Accept<TReturn>(INodeVisitor<TReturn> visitor) where TReturn :IRuntimeObject;

        public enum Operation
        {
            Unknown,
            Expression,
            Block,
            Return,
            Throw,
            Declaration,
            Function,
            If,
            While,
            For,
            This,
        }
    }

    internal class EmptyStatement : Statement
    {
        public EmptyStatement() : base(Operation.Unknown)
        {
        }

        public override TReturn Accept<TReturn>(INodeVisitor<TReturn> visitor)
        {
            return visitor.VisitVoid();
        }
    }
}
