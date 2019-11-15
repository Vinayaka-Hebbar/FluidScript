using FluidScript.Compiler.Reflection;
using FluidScript.Compiler.SyntaxTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace FluidScript.Compiler.Metadata
{
    public sealed class ObjectPrototype : Prototype, IDisposable
    {
        private IList<Reflection.DeclaredMethod> methods;
#if Runtime
        private IDictionary<string, RuntimeObject> constants;

        public readonly Prototype BaseProtoType = Default;
#endif
        private IDictionary<string, Reflection.DeclaredVariable> variables;

        private readonly SyntaxVisitor visitor;

        public override IEnumerable<KeyValuePair<string, DeclaredVariable>> GetVariables()
        {
            return variables ?? (Enumerable.Empty<KeyValuePair<string, DeclaredVariable>>());
        }

        public override IEnumerable<Reflection.DeclaredMethod> GetMethods()
        {
            var declaredMethods = methods ?? (new DeclaredMethod[0]);
#if Runtime
            return Enumerable.Concat(BaseProtoType.GetMethods(), declaredMethods);
#else
            return declaredMethods;
#endif

        }

        public ObjectPrototype(Prototype parent, string name) : base(parent, name, ScopeContext.Type)
        {

        }

        public ObjectPrototype(string name) : base(null, name, ScopeContext.Type)
        {

        }

#if Runtime
        internal ObjectPrototype(IEnumerable<KeyValuePair<string, RuntimeObject>> constants, IEnumerable<KeyValuePair<string, DeclaredVariable>> variables, IEnumerable<DeclaredMethod> methods) : base(null, string.Empty, ScopeContext.Type)
        {
            this.methods = new List<DeclaredMethod>(methods);
            this.constants = constants.ToDictionary(item => item.Key, item => item.Value);
            this.variables = variables.ToDictionary(item => item.Key, item => item.Value);
        }
#endif

        public ObjectPrototype(SyntaxVisitor visitor, string name) : base(visitor.Prototype, name, ScopeContext.Type)
        {
            this.visitor = visitor;
            visitor.Prototype = this;
        }

        internal override DeclaredMethod DeclareMethod(FunctionDeclaration declaration, BodyStatement body)
        {
            if (methods == null)
                methods = new List<DeclaredMethod>();
            var declaredMethod = new Reflection.DeclaredMethod(declaration.Name, methods.Count) { Declaration = declaration, ValueAtTop = body };
            methods.Add(declaredMethod);
            return declaredMethod;
        }

        internal override DeclaredVariable DeclareVariable(string name, Expression expression, VariableType type = VariableType.Local)
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

        public override bool HasVariable(string name)
        {
            return variables != null ? variables.ContainsKey(name) : false;
        }

#if Runtime
        public override void DefineVariable(string name, RuntimeObject value)
        {
            if (variables == null)
                variables = new Dictionary<string, DeclaredVariable>();
            variables.Add(name, new Reflection.DeclaredVariable(name, variables.Count) { DefaultValue = value });
        }

        public override bool HasConstant(string name)
        {
            return constants != null ? constants.ContainsKey(name) : false;
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

        public void DefineVariables(IDictionary<string, RuntimeObject> values)
        {
            if (variables == null)
                variables = new Dictionary<string, DeclaredVariable>();
            foreach (var item in values)
            {
                variables.Add(item.Key, new Reflection.DeclaredVariable(item.Key, variables.Count) { DefaultValue = item.Value });
            }
        }

        public override RuntimeObject GetConstant(string name)
        {
            if (constants == null)
                return RuntimeObject.Undefined;
            return constants[name];
        }

        public override IEnumerable<KeyValuePair<string, RuntimeObject>> GetConstants()
        {
            return constants ?? Enumerable.Empty<KeyValuePair<string, RuntimeObject>>();
        }

        public override RuntimeObject CreateInstance()
        {
            return new Core.ObjectInstance(this);
        }

        internal override IDictionary<object, RuntimeObject> Init(RuntimeObject instance, [Optional] KeyValuePair<object, RuntimeObject> initial)
        {
            var values = BaseProtoType.Init(instance);
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
