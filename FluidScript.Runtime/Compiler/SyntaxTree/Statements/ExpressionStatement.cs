﻿namespace FluidScript.Compiler.SyntaxTree
{
    public class ExpressionStatement : Statement
    {
        public readonly Expression Expression;

        public ExpressionStatement(Expression expression) : base(StatementType.Expression)
        {
            Expression = expression;
        }

#if Runtime
        public override RuntimeObject Evaluate(RuntimeObject instance)
        {
            return Expression.Evaluate(instance);
        }

        internal override RuntimeObject Evaluate(RuntimeObject instance, Metadata.Prototype prototype)
        {
            return Expression.Evaluate(instance);
        }
#endif

        public override void GenerateCode(Reflection.Emit.MethodBodyGenerator generator)
        {
            Expression.Accept(generator).GenerateCode(generator);
        }

        public override string ToString()
        {
            return Expression.ToString();
        }
    }
}
