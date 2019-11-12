using FluidScript.Compiler.Reflection;
using FluidScript.Compiler.SyntaxTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FluidScript.Compiler.Metadata
{
    public sealed class ObjectPrototype : Prototype, IDisposable
    {
        private IList<Reflection.DeclaredMethod> methods;
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

        public void DefineMethod(string name, Emit.ArgumentType[] types, Func<RuntimeObject[], RuntimeObject> onInvoke)
        {
            if (methods == null)
                methods = new List<DeclaredMethod>();
            methods.Add(new DeclaredMethod(name, methods.Count, types) { Delegate = onInvoke });
        }

        public override DeclaredMethod DeclareMethod(FunctionDeclaration declaration, BlockStatement body)
        {
            if (methods == null)
                methods = new List<DeclaredMethod>();
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

        public void DefineVariables(IDictionary<string, RuntimeObject> values)
        {
            if (variables == null)
                variables = new Dictionary<string, DeclaredVariable>();
            foreach (var item in values)
            {
                variables.Add(item.Key, new Reflection.DeclaredVariable(item.Key, variables.Count) { Value = item.Value });
            }
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

        public override Reflection.DeclaredMethod GetMethod(string name, RuntimeType[] types)
        {
            return methods == null ? null : methods.FirstOrDefault(method => method.Name.Equals(name) && Emit.TypeUtils.TypesEqual(method.Types, types));
        }

        internal override RuntimeObject GetConstant(string name)
        {
            if (constants == null)
                return Zero;
            return constants[name];
        }

        public override bool HasVariable(string name)
        {
            return variables != null ? variables.ContainsKey(name) : false;
        }

        public override bool HasConstant(string name)
        {
            return constants != null ? constants.ContainsKey(name) : false;
        }

        public override void Bind<TInstance>()
        {
            if (methods == null)
                methods = new List<DeclaredMethod>();
            var instanceMethods = typeof(TInstance).GetMethods(MemberInvoker.Any)
               .Where(m => m.IsDefined(typeof(Callable), false));
            foreach (var method in instanceMethods)
            {
                var attribute = (Callable)method.GetCustomAttributes(typeof(Callable), false).First();
                DeclaredMethod item = new DeclaredMethod(attribute.Name, methods.Count, attribute.GetArgumentTypes());
                item.Delegate = (args)=> item.Exec(null, method, args);
                methods.Add(item);
            }
        }
    }
}
