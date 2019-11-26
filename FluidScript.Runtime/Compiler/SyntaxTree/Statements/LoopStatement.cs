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

#if Runtime
        public override RuntimeObject Evaluate(RuntimeObject instance)
        {
            var prototype = new Metadata.FunctionPrototype(instance.GetPrototype(), "Loop", Metadata.ScopeContext.Local);
            instance = new Core.LocalInstance(prototype, instance);
            var statement = Statement;
            StatementType nodeType = statement.NodeType;
            var expressions = Expressions;
            if (NodeType == StatementType.Loop)
            {
                if (expressions.Length == 3)
                {
                    for (expressions[0].Evaluate(instance); expressions[1].Evaluate(instance).ToBool(); expressions[2].Evaluate(instance))
                    {
                        var value = statement.Evaluate(instance, prototype);
                        switch (nodeType)
                        {
                            case StatementType.Return:
                                return value;
                            case StatementType.Declaration:
                            case StatementType.Expression:
                                break;
                            default:
                                if (value is object)
                                    return value;
                                break;
                        }
                    }
                }
                else if (expressions.Length == 1)
                {
                    while (expressions[0].Evaluate(instance).ToBool())
                    {
                        var value = statement.Evaluate(instance);
                        switch (nodeType)
                        {
                            case StatementType.Return:
                                return value;
                            case StatementType.Declaration:
                            case StatementType.Expression:
                                break;
                            default:
                                if (value is object)
                                    return value;
                                break;
                        }
                    }
                }
            }
            else if (NodeType == StatementType.Do)
            {
                do
                {
                    var value = statement.Evaluate(instance);
                    switch (nodeType)
                    {
                        case StatementType.Return:
                            return value;
                        case StatementType.Declaration:
                        case StatementType.Expression:
                            break;
                        default:
                            if (value is object)
                                return value;
                            break;
                    }
                } while (expressions[0].Evaluate(instance).ToBool());
            }
            return null;
        }
#endif
    }
}
