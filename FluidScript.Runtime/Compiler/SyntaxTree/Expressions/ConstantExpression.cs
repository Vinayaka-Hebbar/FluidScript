using System.Runtime.InteropServices;
using FluidScript.Compiler.Metadata;

namespace FluidScript.Compiler.SyntaxTree
{
    public class ConstantExpression : NameExpression
    {
        public ConstantExpression(string name, Prototype scope) : base(name, scope, ExpressionType.Identifier)
        {
        }

#if Runtime
        public override RuntimeObject Evaluate([Optional] RuntimeObject instance)
        {
            return Prototype.GetConstant(Name);
        }
#endif

        public override string ToString()
        {
            return string.Concat('_', Name);
        }
    }
}
