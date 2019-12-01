using FluidScript.Reflection;
using FluidScript.Compiler.SyntaxTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace FluidScript.Compiler.Metadata
{
    public sealed class FunctionPrototype : Prototype
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

        public FunctionPrototype(Prototype parent) : base(parent, string.Empty, ScopeContext.Local)
        {
        }

        public FunctionPrototype(Prototype parent, string name, ScopeContext scope) : base(parent, name, scope)
        {
        }

#if Runtime
        public FunctionPrototype(IEnumerable<DeclaredLocalVariable> variables, IEnumerable<DeclaredMember> members, string name) : base(null, name, ScopeContext.Local)
        {
            inner = members.ToList();
            this.variables = variables.ToList();
        }
#endif

        internal override DeclaredMethod DeclareMethod(string name, ParameterInfo[] arguments, ITypeInfo returnType, BlockStatement body)
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

        internal override DeclaredLocalVariable DeclareLocalVariable(string name, ITypeInfo type, Expression expression, VariableFlags attribute = VariableFlags.Default)
        {
            if (variables == null)
                variables = new List<DeclaredLocalVariable>();
            DeclaredLocalVariable variable = variables.FirstOrDefault(item => item.Name == name);
            if (variable == null)
            {
                variable = new DeclaredLocalVariable(name, type, variables.Count, attribute)
                {
                    ValueAtTop = expression,
                    DefaultValue = (attribute & VariableFlags.Constant) == VariableFlags.Constant ? expression.Evaluate(this) : null
                };
                variables.Add(variable);
                return variable;
            }
            if (variable.Attributes == VariableFlags.Constant)
                throw new System.Exception(string.Concat("cannot change readonly value ", name));
            variable.ValueAtTop = expression;
            return variable;
        }

        internal override DeclaredField DeclareField(string name, ITypeInfo type, Expression expression)
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
            DeclareLocalVariable(name, TypeInfo.Any, expression);
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
            variables.Add(new DeclaredLocalVariable(name, value.ReflectedType, variables.Count, VariableFlags.Default) { DefaultValue = value });
        }

        public IEnumerable<DeclaredLocalVariable> GetVariables()
        {
            return this.variables ?? Enumerable.Empty<DeclaredLocalVariable>();
        }

        internal override Instances Init(RuntimeObject obj, [Optional] KeyValuePair<object, RuntimeObject> initial)
        {
            var instance = BaseProtoType.Init(obj, initial);
            if (variables != null)
            {
                foreach (var item in variables)
                {
                    var value = item.DefaultValue;
                    if (value is object)
                    {
                        instance.Add(item.Name, value, (item.Attributes & VariableFlags.Constant) == VariableFlags.Constant);
                    }
                    else
                    {
                        instance.Add(item.Name, RuntimeObject.Undefined);
                    }

                }
            }

            if (inner != null)
            {
                foreach (DeclaredMethod method in inner)
                {
                    instance.AttachFunction(obj, method);
                }
            }
            return instance;
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
