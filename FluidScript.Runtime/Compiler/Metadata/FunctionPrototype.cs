using FluidScript.Compiler.Reflection;
using FluidScript.Compiler.SyntaxTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace FluidScript.Compiler.Metadata
{
    public sealed class FunctionPrototype : Prototype, IDisposable
    {
        private IDictionary<string, DeclaredVariable> variables;
#if Runtime
        private IDictionary<string, RuntimeObject> constants;

        public readonly Prototype BaseProtoType = Default;
#endif
        private IList<DeclaredMethod> inner;

        public readonly SyntaxVisitor visitor;

        public FunctionPrototype() : base(null, string.Empty, ScopeContext.Local)
        {
        }

        public FunctionPrototype(Prototype parent, string name) : base(parent, name, ScopeContext.Local)
        {
        }

        public FunctionPrototype(SyntaxVisitor visitor, string name, ScopeContext context) : base(visitor.Prototype, name, context)
        {
            this.visitor = visitor;
            visitor.Prototype = this;
        }

        public FunctionPrototype(SyntaxVisitor visitor, ScopeContext context) : base(visitor.Prototype, string.Empty, context)
        {
            this.visitor = visitor;
            visitor.Prototype = this;
        }

        internal override DeclaredMethod DeclareMethod(FunctionDeclaration declaration, BodyStatement body)
        {
            if (inner == null)
                inner = new List<DeclaredMethod>();
            var declaredMethod = new Reflection.DeclaredMethod(declaration.Name, inner.Count) { Declaration = declaration, ValueAtTop = body };
            inner.Add(declaredMethod);
            return declaredMethod;
        }

        internal override DeclaredVariable DeclareLocalVariable(string name, Expression expression, VariableType type = VariableType.Local)
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

        internal override DeclaredVariable DeclareVariable(string name, Expression expression, VariableType type = VariableType.Local)
        {
            return Context == ScopeContext.Block ? Parent.DeclareVariable(name, expression, type) : DeclareLocalVariable(name, expression, type);
        }

        public override IEnumerable<KeyValuePair<string, DeclaredVariable>> GetVariables()
        {
            return variables ?? (Enumerable.Empty<KeyValuePair<string, DeclaredVariable>>());
        }

        public override IEnumerable<Reflection.DeclaredMethod> GetMethods()
        {
            var methods = inner ?? (new DeclaredMethod[0]);
#if Runtime
            return Enumerable.Concat(BaseProtoType.GetMethods(), methods);
#else
            return methods;
#endif
        }

        public void Dispose()
        {
            if (visitor != null)
                visitor.Prototype = Parent;
        }

        public override bool HasVariable(string name)
        {
            return Parent != null
                ? variables != null ? variables.ContainsKey(name) ? true : Parent.HasVariable(name) : Parent.HasVariable(name)
                : variables != null ? variables.ContainsKey(name) : false;
        }

#if Runtime
        public override RuntimeObject CreateInstance()
        {
            if (Parent != null)
                return new Core.FunctionInstance(this, Parent.CreateInstance());
            return new Core.FunctionInstance(this, RuntimeObject.Undefined);
        }

        internal Core.FunctionInstance CreateInstance(RuntimeObject obj)
        {
            return new Core.FunctionInstance(this, obj);
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
            variables.Add(name, new Reflection.DeclaredVariable(name, variables.Count) { DefaultValue = value });
        }

        public override RuntimeObject GetConstant(string name)
        {
            if (constants != null && constants.ContainsKey(name))
            {
                return constants[name];
            }
            return Parent.GetConstant(name);
        }

        public override bool HasConstant(string name)
        {
            return constants != null ? constants.ContainsKey(name) ? true : Parent.HasConstant(name) : Parent.HasConstant(name);
        }

        public override IEnumerable<KeyValuePair<string, RuntimeObject>> GetConstants()
        {
            return constants ?? Enumerable.Empty<KeyValuePair<string, RuntimeObject>>();
        }

        internal override IDictionary<object, RuntimeObject> Init(RuntimeObject instance, [Optional] KeyValuePair<object, RuntimeObject> initial)
        {
            var values = BaseProtoType.Init(instance, initial);
            var variables = GetVariables();
            if (variables != null)
            {
                foreach (var item in variables)
                {
                    var value = item.Value.DefaultValue;
                    if (value is object)
                    {
                        values.Add(item.Key, value);
                    }

                }
            }
            var methods = GetMethods();
            if (methods != null)
            {
                foreach (Reflection.DeclaredMethod method in methods)
                {
                    if (method.Store != null)
                    {
                        FunctionGroup list = null;
                        if (values.TryGetValue(method.Name, out RuntimeObject value))
                        {
                            if (value is FunctionGroup)
                            {
                                list = (FunctionGroup)value;
                            }
                        }
                        if (list is null)
                        {
                            list = new FunctionGroup(method.Name);
                            values.Add(method.Name, list);
                        }
                        list.Add(new FunctionReference(instance, method.Arguments, method.ReturnType, method.Store));
                    }

                }
            }
            return values;
        }
#endif
    }
}
