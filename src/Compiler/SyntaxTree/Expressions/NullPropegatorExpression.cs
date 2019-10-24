﻿namespace FluidScript.Compiler.SyntaxTree
{
    public class NullPropegatorExpression : Expression
    {
        public readonly Expression Left;
        public readonly Expression Right;

        public NullPropegatorExpression(Expression left, Expression right) : base(NodeType.Invocation)
        {
            Left = left;
            Right = right;
        }

        public override TReturn Accept<TReturn>(INodeVisitor<TReturn> visitor)
        {
            return visitor.VisitNullPropagator(this);
        }
    }
}