using FluidScript.Compiler.Reflection;
using FluidScript.Compiler.SyntaxTree;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FluidScript.Compiler.Metadata
{
    public class FunctionPrototype : Metadata.Prototype, IDisposable
    {
        private IDictionary<string, Reflection.DeclaredVariable> variables;
        private IDictionary<string, RuntimeObject> constants;
        private IList<Reflection.DeclaredMethod> inner;

        public readonly SyntaxVisitor visitor;

        public FunctionPrototype(Prototype parent) : base(parent, ScopeContext.Local)
        {
        }

        public FunctionPrototype(SyntaxVisitor visitor, ScopeContext context) : base(visitor.Prototype, context)
        {
            this.visitor = visitor;
            visitor.Prototype = this;
        }

        public override DeclaredMethod DeclareMethod(FunctionDeclaration declaration, BlockStatement body)
        {
            if (inner == null)
                inner = new List<DeclaredMethod>();
            var declaredMethod = new Reflection.DeclaredMethod(declaration.Name, inner.Count) { Declaration = declaration, ValueAtTop = body };
            inner.Add(declaredMethod);
            return declaredMethod;
        }

        public override DeclaredVariable DeclareLocalVariable(string name, Expression expression, VariableType type = VariableType.Local)
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

        public override DeclaredVariable DeclareVariable(string name, Expression expression, VariableType type = VariableType.Local)
        {
            return Context == ScopeContext.Block ? Parent.DeclareVariable(name, expression, type) : DeclareLocalVariable(name, expression, type);
        }

        public override void DefineConstant(string name, RuntimeObject value)
        {
            if (Context == ScopeContext.Block)
                Parent.DefineConstant(name, value);
            else
            {
                if (constants == null)
                    constants = new Dictionary<string, RuntimeObject>();
                constants.Add(name, value);
            }
        }

        public override void DefineVariable(string name, RuntimeObject value)
        {
            if (variables == null)
                variables = new Dictionary<string, DeclaredVariable>();
            variables.Add(name, new Reflection.DeclaredVariable(name, variables.Count) { Value = value });
        }

        internal override RuntimeObject GetConstant(string name)
        {
            if (constants != null && constants.ContainsKey(name))
            {
                return constants[name];
            }
            return Parent.GetConstant(name);

        }

        public override Reflection.DeclaredMethod GetMethod(string name, PrimitiveType[] types)
        {
            if (inner == null)
                Parent.GetMethod(name, types);
            return inner.FirstOrDefault(method => method.Name.Equals(name) && Emit.TypeUtils.TypesEqual(method.Types, types));
        }

        public override RuntimeObject this[string name]
        {
            get
            {
                var variable = GetVariable(name);
                if (!ReferenceEquals(variable, null))
                    return variable.Value;
                return Null;
            }
        }

        public override IDictionary<string, DeclaredVariable> Variables
        {
            get
            {
                return variables;
            }
        }

        public void Dispose()
        {
            if (visitor != null)
                visitor.Prototype = Parent;
        }

        internal override bool HasVariable(string name)
        {
            return variables != null ? variables.ContainsKey(name) ? true : Parent.HasVariable(name) : Parent.HasVariable(name);
        }
    }
}
