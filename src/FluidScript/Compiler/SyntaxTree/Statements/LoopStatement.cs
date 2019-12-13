namespace FluidScript.Compiler.SyntaxTree
{
    /// <summary>
    /// Loop Statement
    /// </summary>
    public class LoopStatement : Statement
    {
        public readonly Expression[] Expressions;
        public readonly Statement Statement;
        
        /// <summary>
        /// Initializes a <see cref="LoopStatement"/>
        /// </summary>
        public LoopStatement(Expression[] expressions, Statement statement, StatementType type) : base(type)
        {
            Expressions = expressions;
            Statement = statement;
        }

        /// <inheritdoc/>
        protected internal override void Accept(IStatementVisitor visitor)
        {
            visitor.VisitLoop(this);
        }

        ///<inheritdoc/>
        public override string ToString()
        {
            if (NodeType == StatementType.For)
            {
                return string.Concat("for(", Expressions[0], ";", Expressions[1], ";", Expressions[2], ")\n{", Statement, "\n}\n");
            }
            else if (NodeType == StatementType.While)
            {
                return string.Concat("while(", Expressions[0],")\n{", Statement, "\n}\n");
            }
            else if (NodeType == StatementType.DoWhile)
            {
                return string.Concat("do\n{", Statement, "\n}\nwhile (", Expressions[0], ")");
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
