using FluidScript.Reflection;
using FluidScript.Compiler.SyntaxTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace FluidScript.Compiler.Metadata
{
    public sealed class ObjectPrototype : Prototype
    {
        private ICollection<DeclaredMember> members;

        public readonly Prototype BaseProtoType;
#if Runtime

        private static Prototype prototype;
        internal static Prototype Default
        {
            get
            {
                if (prototype is null)
                {
                    var type = typeof(RuntimeObject);
                    var methods = TypeHelper.GetMethods(type);
                    prototype = new DefaultObjectPrototype(methods);
                }
                return prototype;
            }
        }
#endif

        public override IEnumerable<DeclaredField> GetFields()
        {
            IEnumerable<DeclaredMember> declaredMembers = members ?? (Enumerable.Empty<DeclaredMember>());
            return declaredMembers.OfType<DeclaredField>();
        }

        public override IEnumerable<DeclaredMethod> GetMethods()
        {
            var declaredMethods = members ?? (new DeclaredMember[0]);
#if Runtime
            return Enumerable.Concat(BaseProtoType.GetMethods(), declaredMethods.OfType<DeclaredMethod>());
#else
            return declaredMethods;
#endif
        }

        public override IEnumerable<DeclaredMember> GetMembers()
        {
            var declaredMethods = members ?? (new DeclaredMember[0]);
#if Runtime
            return Enumerable.Concat(BaseProtoType.GetMembers(), declaredMethods);
#else
            return declaredMethods;
#endif
        }

#if Runtime
        public ObjectPrototype(IEnumerable<DeclaredMember> members, Prototype parent, Prototype baseProto, string name) : base(parent, name, ScopeContext.Type)
        {
            BaseProtoType = baseProto;
            this.members = members.ToList();
        }

        public ObjectPrototype(Prototype parent, string name) : base(parent, name, ScopeContext.Type)
        {
            BaseProtoType = Default;
        }
#endif

        internal override DeclaredMethod DeclareMethod(string name, ParameterInfo[] arguments, ITypeInfo returnType, BlockStatement body)
        {
            //todo access
            if (members == null)
                members = new List<DeclaredMember>();
            var declaredMethod = new DeclaredMethod(name, arguments, returnType) { ValueAtTop = body };
            members.Add(declaredMethod);
            return declaredMethod;
        }

        internal override DeclaredField DeclareField(string name, ITypeInfo type, Expression expression)
        {
            if (members == null)
                members = new List<DeclaredMember>();
            var variable = (DeclaredField)members.FirstOrDefault(m => m.Name == name);
            if (variable == null)
            {
                variable = new DeclaredField(name, type);
                members.Add(variable);
            }
            variable.ValueAtTop = expression;
            return variable;
        }

        public override bool HasMember(string name)
        {
            return members != null && members.Any(m => m.Name == name);
        }

#if Runtime
        public override void DefineVariable(string name, RuntimeObject value)
        {
            if (IsSealed)
                throw new Exception(string.Concat("can't declared a variable ", name, " inside " + Name));
            if (members == null)
                members = new List<DeclaredMember>();
            members.Add(new DeclaredField(name, value.ReflectedType) { DefaultValue = value });
        }

        public void DefineVariables(IDictionary<string, RuntimeObject> values)
        {
            if (members == null)
                members = new List<DeclaredMember>();
            foreach (var item in values)
            {
                members.Add(new DeclaredField(item.Key, item.Value.ReflectedType) { DefaultValue = item.Value });
            }
        }

        public override RuntimeObject CreateInstance()
        {
            return new RuntimeObject(this);
        }

        internal override Instances Init(RuntimeObject instance, [Optional] KeyValuePair<object, RuntimeObject> initial)
        {
            //todo override
            Instances values = BaseProtoType.Init(instance);
            if (members != null)
            {
                foreach (var member in members)
                {
                    switch (member)
                    {
                        case DeclaredMethod method:
                            values.AttachFunction(instance, method);
                            break;
                        case DeclaredField field:
                            var value = field.DefaultValue;
                            if (value is null)
                                value = RuntimeObject.Undefined;
                            values.Add(field.Name, value, field.IsReadOnly);
                            break;
                        case DeclaredProperty property:
                            if (property.Getter != null)
                                values.AttachFunction(instance, property.Getter);
                            if (property.Setter != null)
                                values.AttachFunction(instance, property.Setter);
                            break;

                    }

                }
            }
            return values;
        }

        internal override Prototype Merge(Prototype prototype2)
        {
            var members = Enumerable.Concat(GetMethods(), prototype2.GetMethods());
            return new ObjectPrototype(members, null, Default, string.Concat(Name, "_", prototype2.Name));
        }
#endif

    }
}
