using FluidScript.Compiler.Reflection;
using FluidScript.Compiler.SyntaxTree;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FluidScript.Compiler.Metadata
{
    public class ObjectPrototype : Prototype, IDisposable
    {
        //todo lazy init
        private readonly IList<Reflection.DeclaredMethod> methods = new List<Reflection.DeclaredMethod>();
        private IDictionary<string, RuntimeObject> constants;
        private IDictionary<string, Reflection.DeclaredVariable> variables;

        private readonly SyntaxVisitor visitor;

        public override IDictionary<string, DeclaredVariable> Variables => variables;

        public ObjectPrototype(Prototype parent) : base(parent, ScopeContext.Type)
        {

        }

        public ObjectPrototype() : base(null, ScopeContext.Type)
        {

        }

        public ObjectPrototype(SyntaxVisitor visitor) : base(visitor.Prototype, ScopeContext.Type)
        {
            this.visitor = visitor;
            visitor.Prototype = this;
        }

        public void DefineMethod(string name, PrimitiveType[] types, Func<RuntimeObject[], RuntimeObject> onInvoke)
        {
            methods.Add(new DeclaredMethod(name, methods.Count, types) { Delegate = onInvoke });
        }


        public override DeclaredMethod DeclareMethod(FunctionDeclaration declaration, BlockStatement body)
        {
            var declaredMethod = new Reflection.DeclaredMethod(declaration.Name, methods.Count) { Declaration = declaration, ValueAtTop = body };
            methods.Add(declaredMethod);
            return declaredMethod;
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
                visitor.Prototype = Parent;
        }

        public override Reflection.DeclaredMethod GetMethod(string name, PrimitiveType[] types)
        {
            return methods.FirstOrDefault(method => method.Name.Equals(name) && Emit.TypeUtils.TypesEqual(method.Types, types));
        }

        internal override RuntimeObject GetConstant(string name)
        {
            if (constants == null)
                return Zero;
            return constants[name];
        }

        internal override bool HasVariable(string name)
        {
            return variables != null ? variables.ContainsKey(name) : false;
        }
    }
}
