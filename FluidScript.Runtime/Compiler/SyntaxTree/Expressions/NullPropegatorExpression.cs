using System.Runtime.InteropServices;

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

#if Runtime
        public override RuntimeObject Evaluate(RuntimeObject instance)
        {
            //todo this.x??
            if (Left.NodeType == ExpressionType.Identifier)
            {
                var identifier = (NameExpression)Left;
                var value = identifier.Evaluate(instance);
                if (!value.IsNull())
                    return value;
                var result = Right.Evaluate(instance);
                instance[identifier.Name] = result;
                return result;
            }
            return RuntimeObject.Null;
        }
#endif
    }
}
