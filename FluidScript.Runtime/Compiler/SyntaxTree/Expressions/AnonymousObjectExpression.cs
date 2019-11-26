using System.Linq;

namespace FluidScript.Compiler.SyntaxTree
{
    public class AnonymousObjectExpression : Expression
    {
        public readonly AnonymousObjectMember[] Members;
        public AnonymousObjectExpression(AnonymousObjectMember[] expressions) : base(ExpressionType.Block)
        {
            Members = expressions;
        }

#if Runtime
        public override RuntimeObject Evaluate(RuntimeObject instance)
        {
            var prototype = new Metadata.ObjectPrototype(instance.GetPrototype(), "AnnonymousObject");
            var local = new Core.LocalInstance(prototype, instance["this"]);
            foreach (var member in Members)
            {
                local.Append(member.Name, member.Evaluate(instance));
            }
            return local;
        }
#endif

        public override string ToString()
        {
            return string.Concat("{", string.Join(",", Members.Select(s => s.ToString())), "}");
        }
    }
}
