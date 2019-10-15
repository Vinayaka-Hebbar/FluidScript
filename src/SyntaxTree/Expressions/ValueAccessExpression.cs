using System;

namespace FluidScript.SyntaxTree.Expressions
{
    public class ValueAccessExpression : Expression
    {
        public readonly string Id;
        public readonly Func<IOperationContext, string, Object> Access;
        public readonly Func<IOperationContext, string, bool> CanAccess;

        public ValueAccessExpression(string id, Func<IOperationContext, string, Object> access, Func<IOperationContext, string, bool> canAccess, Operation opCode) : base(opCode)
        {
            Id = id;
            Access = access;
            CanAccess = canAccess;
        }

        public override string ToString()
        {
            return Id;
        }

        public override TReturn Accept<TReturn>(INodeVisitor<TReturn> visitor)
        {
            return visitor.VisitValueAccess(this);
        }
    }

}
