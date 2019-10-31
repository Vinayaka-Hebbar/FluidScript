using FluidScript.Compiler.Reflection;

namespace FluidScript.Compiler.SyntaxTree
{
    public class InitializerExpression : VariableDeclarationExpression
    {
        public readonly Expression Target;


        public InitializerExpression(string name, Expression target, Scopes.Scope scope, DeclaredVariable variable) : base(name, scope, variable)
        {
            Target = target;
        }

        public override TReturn Accept<TReturn>(INodeVisitor<TReturn> visitor)
        {
            return visitor.VisitInitializer(this);
        }
    }
}
