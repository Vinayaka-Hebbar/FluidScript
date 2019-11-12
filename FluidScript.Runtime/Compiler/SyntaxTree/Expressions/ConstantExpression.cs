using FluidScript.Compiler.Metadata;

namespace FluidScript.Compiler.SyntaxTree
{
    public class ConstantExpression : NameExpression
    {
        public ConstantExpression(string name, Prototype scope) : base(name, scope, ExpressionType.Identifier)
        {
        }

        public override RuntimeObject Evaluate()
        {
            return Prototype.GetConstant(Name);
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
