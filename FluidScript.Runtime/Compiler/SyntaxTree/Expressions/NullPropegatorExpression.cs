namespace FluidScript.Compiler.SyntaxTree
{
    public class NullPropegatorExpression : Expression
    {
        public readonly Expression Left;
        public readonly Expression Right;

        public NullPropegatorExpression(Expression left, Expression right) : base(ExpressionType.Invocation)
        {
            Left = left;
            Right = right;
        }

        public override RuntimeObject Evaluate()
        {
            if (Left.NodeType == ExpressionType.Identifier)
            {
                var identifier = (NameExpression)Left;
                var value = identifier.Evaluate();
                if (!value.IsNull())
                    return value;
                var result = Right.Evaluate();
                identifier.Set(result);
                return result;
            }
            return RuntimeObject.Null;
        }
    }
}
