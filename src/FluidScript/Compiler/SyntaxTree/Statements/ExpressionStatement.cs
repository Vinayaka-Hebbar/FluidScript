﻿namespace FluidScript.Compiler.SyntaxTree
{
    public class ExpressionStatement : Statement
    {
        public readonly Expression Expression;

        public ExpressionStatement(Expression expression) : base(StatementType.Expression)
        {
            Expression = expression;
        }

        protected internal override void Accept(IStatementVisitor visitor)
        {
            visitor.VisitExpression(this);
        }

        public override void GenerateCode(Reflection.Emit.MethodBodyGenerator generator)
        {
            Expression expression = Expression.Accept(generator);
            expression.GenerateCode(generator);
            //for void call
            if (expression.NodeType == ExpressionType.Invocation && expression.Type == null)
                generator.Pop();
        }

        public override string ToString()
        {
            return Expression.ToString();
        }
    }
}
