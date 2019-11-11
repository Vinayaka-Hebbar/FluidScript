using FluidScript.Compiler.Scopes;

namespace FluidScript.Compiler.SyntaxTree
{
    public class ConstantExpression : NameExpression
    {
        public ConstantExpression(string name, Scope scope) : base(name, scope, ExpressionType.Identifier)
        {
        }

        public override RuntimeObject Evaluate()
        {
            return Scope.GetConstant(Name);
        }

        internal override void Set(RuntimeObject value)
        {
            throw new System.Exception("tring to modify read only value");
        }

        public override string ToString()
        {
            return string.Concat('_', Name);
        }
    }
}
