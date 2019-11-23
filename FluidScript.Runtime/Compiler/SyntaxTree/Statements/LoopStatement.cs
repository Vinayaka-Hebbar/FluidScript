namespace FluidScript.Compiler.SyntaxTree
{
    public class LoopStatement : Statement
    {
        public readonly Expression[] Expressions;
        public readonly Statement Statement;
        public readonly Metadata.FunctionPrototype Prototype;
        public LoopStatement(Expression[] expressions, Statement statement, Metadata.FunctionPrototype prototype, StatementType type) : base(type)
        {
            Expressions = expressions;
            Statement = statement;
            Prototype = prototype;
        }

#if Runtime
        public override RuntimeObject Evaluate(RuntimeObject instance)
        {
            instance = new Core.LocalInstance(Prototype, instance);
            var statement = Statement;
            var expressions = Expressions;
            if (NodeType == StatementType.Loop)
            {
                if (expressions.Length == 3)
                {
                    for (expressions[0].Evaluate(instance); expressions[1].Evaluate(instance).ToBool(); expressions[2].Evaluate(instance))
                    {
                        StatementType nodeType = statement.NodeType;
                        var value = statement.Evaluate(instance);
                        if (nodeType == StatementType.Return)
                        {
                            return value;
                        }
                        if (nodeType != StatementType.Expression)
                        {
                            if (value is object)
                            {
                                return value;
                            }
                        }
                    }
                }
                else if (expressions.Length == 1)
                {
                    while (expressions[0].Evaluate(instance).ToBool())
                    {
                        StatementType nodeType = statement.NodeType;
                        var value = statement.Evaluate(instance);
                        if (nodeType == StatementType.Return)
                        {
                            return value;
                        }
                        if (nodeType != StatementType.Expression)
                        {
                            if (value is object)
                            {
                                return value;
                            }
                        }
                    }
                }
            }
            else if (NodeType == StatementType.Do)
            {
                do
                {
                    StatementType nodeType = statement.NodeType;
                    var value = statement.Evaluate(instance);
                    if (nodeType == StatementType.Return)
                    {
                        return value;
                    }
                    if (nodeType != StatementType.Expression)
                    {
                        if (value is object)
                        {
                            return value;
                        }
                    }
                } while (expressions[0].Evaluate(instance).ToBool());
            }
            return null;
        }
#endif
    }
}
