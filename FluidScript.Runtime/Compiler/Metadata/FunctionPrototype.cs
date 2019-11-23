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
        private ICollection<DeclaredLocalVariable> variables;
#if Runtime
        public readonly Prototype BaseProtoType = Default;


        private static Prototype prototype;
        internal static Prototype Default
        {
            get
            {
                if (prototype is null)
                {
                    prototype = new DefaultObjectPrototype(new DeclaredMethod[0]);
                }
                return prototype;
            }
        }
#endif
        private ICollection<DeclaredMember> inner;

        public DeclaredLocalVariable GetLocalVariable(string name)
        {
            if (variables == null)
                return null;
            return variables.FirstOrDefault(v => v.Name == name);
        }

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

#if Runtime
        public FunctionPrototype(IEnumerable<DeclaredLocalVariable> variables, IEnumerable<DeclaredMember> members, string name) : base(null, name, ScopeContext.Local)
        {
            inner = members.ToList();
            this.variables = variables.ToList();
        }
#endif

        internal override DeclaredMethod DeclareMethod(string name, ArgumentInfo[] arguments, Emit.TypeName returnType, BodyStatement body)
        {
            if (inner == null)
                inner = new List<DeclaredMember>();
            DeclaredMethod declaredMethod = new DeclaredMethod(name, arguments, returnType)
            {
                ValueAtTop = body,
                Attributes = System.Reflection.MethodAttributes.Public
            };
            inner.Add(declaredMethod);
            return declaredMethod;
        }

        internal override DeclaredLocalVariable DeclareLocalVariable(string name, Emit.TypeName type, Expression expression, VariableAttributes attribute = VariableAttributes.Default)
        {
            if (variables == null)
                variables = new List<DeclaredLocalVariable>();
            DeclaredLocalVariable variable = variables.FirstOrDefault(item => item.Name == name);
            if (variable == null)
            {
                variable = new DeclaredLocalVariable(name, type, variables.Count, attribute)
                {
                    ValueAtTop = expression,
                    DefaultValue = (attribute & VariableAttributes.Constant) == VariableAttributes.Constant ? expression.Evaluate(this) : null
                };
                variables.Add(variable);
                return variable;
            }
            if (variable.Attributes == VariableAttributes.Constant)
                throw new System.Exception(string.Concat("cannot change readonly value ", name));
            variable.ValueAtTop = expression;
            return variable;
        }

        internal override DeclaredField DeclareField(string name, Emit.TypeName type, Expression expression)
        {
            return Parent.DeclareField(name, type, expression);
        }

        /// <summary>
        /// Gets all the declared fields
        /// </summary>
        public override IEnumerable<DeclaredField> GetFields()
        {
            return Enumerable.Empty<DeclaredField>();
        }

        /// <summary>
        /// Get all the declared methods
        /// </summary>
        public override IEnumerable<DeclaredMethod> GetMethods()
        {
            var methods = inner ?? (new DeclaredMember[0]);
#if Runtime
            return Enumerable.Concat(BaseProtoType.GetMethods(), methods.OfType<DeclaredMethod>());
#else
            return methods;
#endif
        }


        public override IEnumerable<DeclaredMember> GetMembers()
        {
            return inner ?? (new DeclaredMember[0]);
        }

        void IDisposable.Dispose()
        {
            if (visitor != null)
                visitor.Prototype = Parent;
        }

        public override bool HasMember(string name)
        {
            return Parent is object
                ? variables != null ? variables.Any(item => item.Name == name) ? true : Parent.HasMember(name) : Parent.HasMember(name)
                : variables != null ? variables.Any(item => item.Name == name) : false;
        }

#if Runtime
        internal override void DeclareVariable(string name, Expression expression)
        {
            if (IsSealed)
                throw new Exception(string.Concat("can't declared a variable ", name, " inside " + Name));
            if (Context == ScopeContext.Block)
                Parent.DeclareVariable(name, expression);
            DeclareLocalVariable(name, Emit.TypeName.Any, expression);
        }

        public override RuntimeObject CreateInstance()
        {
            if (Parent is object)
                return new Core.FunctionInstance(this, Parent.CreateInstance());
            return new Core.FunctionInstance(this, RuntimeObject.Undefined);
        }

        internal Core.FunctionInstance CreateInstance(RuntimeObject obj)
        {
            return new Core.FunctionInstance(this, obj);
        }

        public override void DefineVariable(string name, RuntimeObject value)
        {
            if (IsSealed)
                throw new Exception(string.Concat("can't declared a variable ", name, " inside " + Name));
            if (variables == null)
                variables = new List<DeclaredLocalVariable>();
            variables.Add(new DeclaredLocalVariable(name, value.ReflectedType, variables.Count, VariableAttributes.Default) { DefaultValue = value });
        }

        public IEnumerable<DeclaredLocalVariable> GetVariables()
        {
            return this.variables ?? Enumerable.Empty<DeclaredLocalVariable>();
        }

        internal override Instances Init(RuntimeObject instance, [Optional] KeyValuePair<object, RuntimeObject> initial)
        {
            var values = BaseProtoType.Init(instance, initial);
            if (variables != null)
            {
                foreach (var item in variables)
                {
                    var value = item.DefaultValue;
                    if (value is object)
                    {
                        values.Add(item.Name, value, (item.Attributes & VariableAttributes.Constant) == VariableAttributes.Constant);
                    }
                    else
                    {
                        values.Add(item.Name, RuntimeObject.Undefined);
                    }

                }
            }

            if (inner != null)
            {
                foreach (DeclaredMethod method in inner)
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
                        if (method.Default is null)
                            list.Add(new FunctionReference(instance, method.Arguments, method.ReflectedReturnType, method.Store));
                        else
                            list.Add(method.Default);
                    }
                }
            }
            return values;
        }

        internal override Prototype Merge(Prototype prototype2)
        {
            var variables = Enumerable.Concat(GetVariables(), ((FunctionPrototype)prototype2).GetVariables());
            var methods = Enumerable.Concat(GetMethods(), prototype2.GetMethods());
            return new FunctionPrototype(variables, methods, string.Concat(Name, "_", prototype2.Name));
        }

#endif
    }
}
