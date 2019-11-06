using FluidScript.Compiler.Reflection;
using FluidScript.Compiler.SyntaxTree;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FluidScript.Compiler.Scopes
{
    public class ObjectScope : Scope, IDisposable
    {
        private readonly IList<Reflection.DeclaredMethod> methods = new List<Reflection.DeclaredMethod>();
        private readonly IDictionary<string, RuntimeObject> constants = new Dictionary<string, RuntimeObject>();
        private IDictionary<string, Reflection.DeclaredVariable> variables;

        private readonly SyntaxVisitor visitor;

        public override IDictionary<string, DeclaredVariable> Variables => variables;

        public ObjectScope(Scope parent) : base(parent, ScopeContext.Type)
        {

        }

        public ObjectScope(SyntaxVisitor visitor) : base(visitor.Scope, ScopeContext.Type)
        {
            this.visitor = visitor;
        }

        internal void DefineMethod(string name, PrimitiveType[] types, Func<RuntimeObject[], RuntimeObject> onInvoke)
        {
            methods.Add(new DeclaredMethod(name, methods.Count, types) { Delegate = onInvoke });
        }

        internal void DefineVariable(string name, RuntimeObject value)
        {
            if (variables == null)
                variables = new Dictionary<string, DeclaredVariable>();
            variables.Add(name, new DeclaredVariable(name, variables.Count) { Value = value });
        }

        public Reflection.DeclaredMethod DeclareMethod(string name, SyntaxTree.FunctionDeclaration declaration, SyntaxTree.BlockStatement body)
        {
            var declaredMethod = new Reflection.DeclaredMethod(name, methods.Count) { Declaration = declaration, ValueAtTop = body };
            methods.Add(declaredMethod);
            return declaredMethod;
        }

        public void DefineConstant(string name, RuntimeObject value)
        {
            if (constants.ContainsKey(name))
            {
                if (value.IsNull() == false)
                {
                    constants[name] = value;
                }
                return;
            }
            constants.Add(name, value);
        }

        public override DeclaredVariable DeclareVariable(string name, Expression expression, VariableType type = VariableType.Local)
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

        public void Dispose()
        {
            if (visitor != null)
                visitor.Scope = Parent;
        }

        internal Reflection.DeclaredMethod GetMethod(string name, PrimitiveType[] types)
        {
            return methods.FirstOrDefault(method => method.Name.Equals(name) && TypesEqual(method.Types, types));
        }

        private static bool TypesEqual(PrimitiveType[] left, PrimitiveType[] right)
        {
            bool isEquals = true;
            for (int i = 0; i < left.Length; i++)
            {
                if(left[i] != right[i])
                {
                    isEquals = false;
                    break;
                }
            }
            return isEquals;
        }
    }
}
