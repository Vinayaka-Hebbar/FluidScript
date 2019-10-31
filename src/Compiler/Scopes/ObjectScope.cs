using FluidScript.Compiler.Reflection;
using FluidScript.Compiler.SyntaxTree;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace FluidScript.Compiler.Scopes
{
    public class ObjectScope : Scope, System.IDisposable
    {
        private List<DeclaredMember> localMembers;
        private readonly SyntaxVisitor visitor;
        public ObjectScope(SyntaxVisitor visitor) : base(visitor.Scope, false)
        {
            this.visitor = visitor;
            visitor.Scope = this;
        }
        public ObjectScope(Scope parentScope) : base(parentScope, false)
        {
        }

        internal override DeclaredMember DeclareMember(Declaration declaration, Reflection.BindingFlags binding, Reflection.MemberTypes memberType, Statement statement = null)
        {
            if (localMembers == null)
                localMembers = new List<DeclaredMember>();
            DeclaredMember member = localMembers.Find((obj) => obj.Name.Equals(declaration.Name));
            if (member == null)
            {
                switch (memberType)
                {
                    case Reflection.MemberTypes.Field:
                        member = new DeclaredField(declaration, localMembers.Count, binding);
                        break;
                    case Reflection.MemberTypes.Function:
                        member = new DeclaredMethod(declaration, localMembers.Count, binding);
                        break;
                    case Reflection.MemberTypes.Property:
                        member = new DeclaredProperty(declaration, localMembers.Count, binding);
                        break;
                }
                localMembers.Add(member);
            }
            member.ValueAtTop = statement;
            return member;
        }

        internal IEnumerable<DeclaredMember> Members
        {
            get
            {
                if (localMembers == null)
                    return System.Linq.Enumerable.Empty<DeclaredMember>();
                return localMembers;
            }
        }
        public void Dispose()
        {
            if (visitor != null)
                visitor.Scope = ParentScope;
        }
    }
}
