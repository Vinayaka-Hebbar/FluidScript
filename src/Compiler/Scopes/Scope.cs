
using FluidScript.Compiler.Emit;
using FluidScript.Compiler.Reflection;
using FluidScript.Compiler.SyntaxTree;

namespace FluidScript.Compiler.Scopes
{
    public abstract class Scope
    {
        public readonly string Name;
        public readonly Scope ParentScope;
        public readonly bool CanDeclareVariables;

        public abstract ScopeContext Context { get; }
        protected Scope(Scope scope, string name, bool canDeclareVariables)
        {
            Name = name;
            ParentScope = scope;
            CanDeclareVariables = canDeclareVariables;
        }

        internal virtual DeclaredMember DeclareMember(Declaration declaration, BindingFlags binding, MemberTypes memberType, Statement statement = null)
        {
            throw new System.InvalidOperationException("Cannot delcare member here");
        }

        internal virtual DeclaredVariable DeclareVariable(string name, TypeName typeName, Expression expression = null, VariableType variableType = VariableType.Local)
        {
            throw new System.InvalidOperationException("Cannot declare variable here");
        }

        internal virtual void GenerateDeclarations(ILGenerator generator, MethodOptimizationInfo info)
        {

        }
    }
}