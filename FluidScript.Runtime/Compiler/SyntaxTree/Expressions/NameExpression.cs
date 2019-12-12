using FluidScript.Reflection.Emit;
using System.Linq;

namespace FluidScript.Compiler.SyntaxTree
{
    public sealed class NameExpression : Expression
    {
        public readonly string Name;

        public Binding Binding { get; internal set; }

        public NameExpression(string name, ExpressionType opCode) : base(opCode)
        {
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }

        public override TResult Accept<TResult>(IExpressionVisitor<TResult> visitor)
        {
            return visitor.VisitMember(this);
        }

        public override void GenerateCode(MethodBodyGenerator generator)
        {
            if (Binding.IsMember && generator.Method.IsStatic == false)
                generator.LoadArgument(0);
            Binding.GenerateGet(generator);
        }

    }
}
