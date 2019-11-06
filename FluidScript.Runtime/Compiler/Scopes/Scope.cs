using System.Collections.Generic;

namespace FluidScript.Compiler.Scopes
{
    public abstract class Scope
    {
        public readonly Scope Parent;
        public readonly ScopeContext Context;

        public Scope(Scope parent, ScopeContext context)
        {
            Parent = parent;
            Context = context;
        }

        public Reflection.DeclaredMethod DeclareMethod(SyntaxTree.FunctionDeclaration declaration, SyntaxTree.BlockStatement body)
        {
            throw new System.Exception("Can't declare method inside " + GetType());
        }

        public virtual Reflection.DeclaredVariable DeclareLocalVariable(string name, SyntaxTree.Expression expression, Reflection.VariableType type = Reflection.VariableType.Local)
        {
            throw new System.Exception("Can't declare local variable inside " + GetType());
        }

        public virtual Reflection.DeclaredVariable DeclareVariable(string name, SyntaxTree.Expression expression, Reflection.VariableType type = Reflection.VariableType.Local)
        {
            throw new System.Exception("Can't declare variable inside " + GetType());
        }

        public abstract IDictionary<string, Reflection.DeclaredVariable> Variables { get; }

        public Reflection.DeclaredVariable GetVariable(string name)
        {
            if (Variables != null && Variables.ContainsKey(name))
                return Variables[name];
            if (Parent != null)
            {
                return Parent.GetVariable(name);
            }
            throw new System.Exception(string.Format("Variable {0}", name));
        }

    }
}
