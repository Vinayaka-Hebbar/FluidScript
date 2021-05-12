using FluidScript.Utils;

namespace FluidScript.Compiler.SyntaxTree
{
    public sealed class AnonymousObjectExpression : Expression
    {
        public readonly NodeList<AnonymousObjectMember> Members;
        public AnonymousObjectExpression(NodeList<AnonymousObjectMember> expressions) : base(ExpressionType.AnonymousObject)
        {
            Members = expressions;
        }

        public override TResult Accept<TResult>(IExpressionVisitor<TResult> visitor)
        {
            return visitor.VisitAnonymousObject(this);
        }

        public override void GenerateCode(Emit.MethodBodyGenerator generator, Emit.MethodCompileOption option)
        {
            generator.NewObject(ReflectionHelpers.AnonymousObj);
            for (int i = 0; i < Members.Count; i++)
            {
                var member = Members[i];
                generator.Duplicate();
                generator.LoadString(member.Name);
                member.Expression.GenerateCode(generator, AssignOption);
                if (member.Expression.Type.IsValueType)
                    generator.Box(member.Expression.Type);
                generator.CallVirtual(ReflectionHelpers.AnonymousObj_SetItem);
            }
        }

        public override string ToString()
        {
            return string.Concat("{", string.Join(",", Members.Map(s => s.ToString())), "}");
        }
    }
}
