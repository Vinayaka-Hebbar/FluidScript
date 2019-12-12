namespace FluidScript.Compiler.SyntaxTree
{
    public class LoopStatement : Statement
    {
        public readonly Expression[] Expressions;
        public readonly Statement Statement;
        public LoopStatement(Expression[] expressions, Statement statement, StatementType type) : base(type)
        {
            Expressions = expressions;
            Statement = statement;
        }
    }
}
