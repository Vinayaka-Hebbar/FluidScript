﻿namespace FluidScript.Compiler.SyntaxTree
{
    public class TernaryOperatorExpression : Expression
    {
        public readonly Expression First;

        public readonly Expression Second;

        public readonly Expression Third;

        public TernaryOperatorExpression(Expression first, Expression second, Expression third) : base(NodeType.Question)
        {
            First = first;
            Second = second;
            Third = third;
        }

        public override TReturn Accept<TReturn>(INodeVisitor<TReturn> visitor)
        {
            return First.Accept(visitor).ToBool() ? Second.Accept(visitor) : Third.Accept(visitor);
        }
    }
}