using FluidScript.Compiler.Reflection;
using FluidScript.Compiler.SyntaxTree;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public override ScopeContext Context { get; } = ScopeContext.Type;

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

        internal DeclaredMethod GetMethod(string name, Type[] argumentTypes)
        {
            var methods = localMembers.Where(member => member.MemberType == System.Reflection.MemberTypes.Method && member.Name.Equals(name))
                .Select(member => (DeclaredMethod)member);
            foreach (var item in methods)
            {
                System.Type[] array = ((FunctionDeclaration)item.Declaration).ArgumentTypes;
                if (argumentTypes.Length == array.Length)
                {
                    bool status = true;
                    for (int i = 0; i < array.Length; i++)
                    {
                        System.Type paramater = array[i];
                        if (paramater != argumentTypes[i])
                        {
                            status = false;
                            break;
                        }
                    }
                    if (status == true)
                    {
                        return item;
                    }
                }
            }
            return null;
        }

        internal DeclaredField GetField(string name)
        {
            return (DeclaredField)localMembers.FirstOrDefault(member => member.MemberType == System.Reflection.MemberTypes.Field && member.Name.Equals(name));
        }

        internal DeclaredProperty GetProperty(string name)
        {
            return (DeclaredProperty)localMembers.FirstOrDefault(member => member.MemberType == System.Reflection.MemberTypes.Property && member.Name.Equals(name));
        }

        internal DeclaredMember GetMember(string name)
        {
            return localMembers.FirstOrDefault(member => member.Name.Equals(name));
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

        public IEnumerable<DeclaredMember> Fields => localMembers.Where(member => member.IsField);

        public void Dispose()
        {
            if (visitor != null)
                visitor.Scope = ParentScope;
        }
    }
}
