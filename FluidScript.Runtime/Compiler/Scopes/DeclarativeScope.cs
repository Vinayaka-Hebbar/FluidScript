using FluidScript.Compiler.Reflection;
using FluidScript.Compiler.SyntaxTree;
using System;
using System.Collections.Generic;

namespace FluidScript.Compiler.Scopes
{
    public class DeclarativeScope : Scopes.Scope, IDisposable
    {
        private IDictionary<string, Reflection.DeclaredVariable> variables;
        public readonly SyntaxVisitor visitor;
        public DeclarativeScope(Scope parent) : base(parent, ScopeContext.Local)
        {
        }

        public DeclarativeScope(SyntaxVisitor visitor) : base(visitor.Scope, ScopeContext.Local)
        {
            this.visitor = visitor;
        }

        public override DeclaredVariable DeclareLocalVariable(string name, Expression expression, VariableType type = VariableType.Local)
        {
            if (variables == null)
                variables = new Dictionary<string, Reflection.DeclaredVariable>();
            if (variables.TryGetValue(name, out Reflection.DeclaredVariable variable) == false)
            {
                variable = new DeclaredVariable(name, variables.Count, type);
            }
            variable.ValueAtTop = expression;
            return variable;
        }

        public override DeclaredVariable DeclareVariable(string name, Expression expression, VariableType type = VariableType.Local)
        {
            return Parent.DeclareVariable(name, expression, type);
        }

        public override IDictionary<string, DeclaredVariable> Variables => variables;

        public void Dispose()
        {
            if (visitor != null)
                visitor.Scope = Parent;
        }
    }
}
