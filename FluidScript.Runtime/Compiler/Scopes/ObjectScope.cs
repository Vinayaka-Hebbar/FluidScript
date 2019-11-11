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
        private IDictionary<string, RuntimeObject> constants;
        private IDictionary<string, Reflection.DeclaredVariable> variables;

        private readonly SyntaxVisitor visitor;

        public override IDictionary<string, DeclaredVariable> Variables => variables;

        public ObjectScope(Scope parent) : base(parent, ScopeContext.Type)
        {

        }

        public ObjectScope() : base(null, ScopeContext.Type)
        {

        }

        public ObjectScope(SyntaxVisitor visitor) : base(visitor.Scope, ScopeContext.Type)
        {
            this.visitor = visitor;
            visitor.Scope = this;
        }

        public void DefineMethod(string name, PrimitiveType[] types, Func<RuntimeObject[], RuntimeObject> onInvoke)
        {
            methods.Add(new DeclaredMethod(name, methods.Count, types) { Delegate = onInvoke });
        }

        public override void DefineVariable(string name, RuntimeObject value)
        {
            if (variables == null)
                variables = new Dictionary<string, DeclaredVariable>();
            variables.Add(name, new Reflection.DeclaredVariable(name, variables.Count) { Value = value });
        }

        public override void DefineConstant(string name, RuntimeObject value)
        {
            if (constants == null)
                constants = new Dictionary<string, RuntimeObject>();
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

        public Reflection.DeclaredMethod DeclareMethod(string name, SyntaxTree.FunctionDeclaration declaration, SyntaxTree.BlockStatement body)
        {
            var declaredMethod = new Reflection.DeclaredMethod(name, methods.Count) { Declaration = declaration, ValueAtTop = body };
            methods.Add(declaredMethod);
            return declaredMethod;
        }

        public override DeclaredVariable DeclareVariable(string name, Expression expression, VariableType type = VariableType.Local)
        {
            if (variables == null)
                variables = new Dictionary<string, Reflection.DeclaredVariable>();
            if (variables.TryGetValue(name, out Reflection.DeclaredVariable variable) == false)
            {
                variable = new DeclaredVariable(name, variables.Count, type);
                variables.Add(name, variable);
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

        internal override RuntimeObject GetConstant(string name)
        {
            if (constants == null)
                return RuntimeObject.Zero;
            return constants[name];
        }

        private static bool TypesEqual(PrimitiveType[] types, PrimitiveType[] calledTypes)
        {
            bool isEquals = true;
            for (int i = 0; i < types.Length; i++)
            {
                if ((calledTypes[i] & types[i]) != types[i])
                {
                    isEquals = false;
                    break;
                }
            }
            return isEquals;
        }

        internal override bool HasVariable(string name)
        {
            return variables != null ? variables.ContainsKey(name) : false;
        }
    }
}
