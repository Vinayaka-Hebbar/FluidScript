using FluidScript.Compiler.Emit;
using FluidScript.Compiler.Reflection;
using FluidScript.Compiler.SyntaxTree;
using System.Collections.Generic;

namespace FluidScript.Compiler.Scopes
{
    public class DeclarativeScope : Scope, System.IDisposable
    {
        private readonly SyntaxVisitor visitor;
        private List<DeclaredMember> localMembers;
        private IDictionary<string, DeclaredVariable> localVaribales;
        public DeclarativeScope(SyntaxVisitor visitor) : base(visitor.Scope, true)
        {
            this.visitor = visitor;
            visitor.Scope = this;
        }

        public DeclarativeScope(Scope parentScope) : base(parentScope, true)
        {
        }

        public override ScopeContext Context { get; } = ScopeContext.Local;

        public void Dispose()
        {
            if (visitor != null)
                visitor.Scope = ParentScope;
        }

        internal override DeclaredMember DeclareMember(Declaration declaration, BindingFlags binding, MemberTypes memberType, Statement statement = null)
        {
            //todo virtual abstract, override
            if (localMembers == null)
                localMembers = new List<DeclaredMember>();
            DeclaredMember member;
            switch (memberType)
            {
                case MemberTypes.Function:
                    member = new DeclaredMethod(declaration, localMembers.Count, binding)
                    {
                        ValueAtTop = statement
                    };
                    break;
                default:
                    throw new System.InvalidOperationException(string.Format("cannot declare {0} here", memberType));
            }
            localMembers.Add(member);
            return member;
        }

        internal DeclaredVariable GetVariable(string name)
        {
            if (localVaribales.ContainsKey(name))
                return localVaribales[name];
            return null;
        }

        internal override DeclaredVariable DeclareVariable(string name, TypeName typeName, Expression expression = null, VariableType variableType = VariableType.Local)
        {
            if (localVaribales == null)
                localVaribales = new Dictionary<string, DeclaredVariable>();
            if (localVaribales.ContainsKey(name))
                throw new System.InvalidOperationException(string.Format("{0} already present", name));
            var variable = new DeclaredVariable(name, typeName, localVaribales.Count, expression, variableType);
            localVaribales.Add(name, variable);
            return variable;
        }

        internal override void GenerateDeclarations(ILGenerator generator, MethodOptimizationInfo info)
        {
            if (localMembers != null)
            {
                foreach (var variable in localVaribales.Values)
                {
                    if (variable.ValueAtTop == null)
                    {

                    }
                }
            }
        }
    }
}
