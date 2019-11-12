using FluidScript.Compiler.Reflection;
using FluidScript.Compiler.SyntaxTree;
using System.Collections.Generic;

namespace FluidScript.Compiler.Metadata
{
    public class GlobalScope : Scope
    {
        private IList<DeclaredType> declaredTypes;
        public GlobalScope(string name) : base(null, name, false)
        {
        }

        public override ScopeContext Context { get; } = ScopeContext.Global;

        internal override DeclaredMember DeclareMember(Declaration declaration, BindingFlags binding, MemberTypes memberType, Statement statement = null)
        {
            if (declaredTypes == null)
                declaredTypes = new List<DeclaredType>();
            if (memberType == MemberTypes.Type)
            {
                return new DeclaredType(declaration, declaredTypes.Count, binding);
            }
            throw new System.Exception("Cannot declaration member other than type");
        }
    }
}
