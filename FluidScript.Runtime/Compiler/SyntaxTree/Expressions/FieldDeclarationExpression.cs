using System.Runtime.InteropServices;

namespace FluidScript.Compiler.SyntaxTree
{
    public class FieldDeclarationExpression : Expression
    {
        public readonly string Name;
        public readonly Reflection.DeclaredField Member;

        public FieldDeclarationExpression(string name, Reflection.DeclaredField member) : base(ExpressionType.Declaration)
        {
            Name = name;
            Member = member;
        }

#if Runtime
        public override RuntimeObject Evaluate([Optional] RuntimeObject instance)
        {
            var value = Member.Evaluate(instance);
            instance[Member.Name] = value;
            return value;
        }
#endif
    }
}
