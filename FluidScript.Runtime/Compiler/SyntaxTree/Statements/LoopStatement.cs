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

        public override RuntimeObject Evaluate()
        {
            var statement = Statement;
            var expressions = Expressions;
            RuntimeObject result = RuntimeObject.Null;
            if (NodeType == StatementType.Loop)
            {
                if (expressions.Length == 3)
                {
                    for (expressions[0].Evaluate(); expressions[1].Evaluate().ToBool(); expressions[2].Evaluate())
                    {
                        if (statement.NodeType == StatementType.Return)
                        {
                            result = statement.Evaluate();
                            break;
                        }

                        result = statement.Evaluate();
                        if (result.IsReturn)
                            break;
                    }
                    return result;
                }
                if (expressions.Length == 1)
                {
                    while (expressions[0].Evaluate().ToBool())
                    {
                        if (statement.NodeType == StatementType.Return)
                        {
                            result = statement.Evaluate();
                        }

                        result = statement.Evaluate();
                        if (result.IsReturn)
                        {
                            break;
                        }
                    }
                }
            }
            else if (NodeType == StatementType.Do)
            {
                do
                {
                    if (statement.NodeType == StatementType.Return)
                    {
                        result = statement.Evaluate();
                    }

                    result = statement.Evaluate();
                    if (result.IsReturn)
                    {
                        break;
                    }
                } while (expressions[0].Evaluate().ToBool());
            }
            return result;
        }
    }
}
