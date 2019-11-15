using FluidScript.Compiler.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace FluidScript.Compiler.Metadata
{
    internal sealed class DefaultObjectPrototype : Prototype
    {
        public readonly IList<DeclaredMethod> methods;
        public DefaultObjectPrototype(IEnumerable<DeclaredMethod> methods) : base(null, "RuntimeObject", ScopeContext.Type)
        {
            this.methods = new List<DeclaredMethod>(methods);
        }

        public override bool HasVariable(string name)
        {
            return false;
        }

        public override IEnumerable<DeclaredMethod> GetMethods()
        {
            return methods;
        }

        public override IEnumerable<KeyValuePair<string, DeclaredVariable>> GetVariables()
        {
            return Enumerable.Empty<KeyValuePair<string, DeclaredVariable>>();
        }

#if Runtime
        public override RuntimeObject CreateInstance()
        {
            return new Core.ObjectInstance(this);
        }

        public override bool HasConstant(string name)
        {
            return false;
        }

        public override void DefineConstant(string name, RuntimeObject value)
        {
        }

        public override void DefineVariable(string name, RuntimeObject value)
        {
        }

        public override IEnumerable<KeyValuePair<string, RuntimeObject>> GetConstants()
        {
            return Enumerable.Empty<KeyValuePair<string, RuntimeObject>>();
        }

        internal override IDictionary<object, RuntimeObject> Init(RuntimeObject instance, [Optional] KeyValuePair<object, RuntimeObject> initial)
        {
            var values = new Dictionary<object, RuntimeObject>();
            if (initial.Key != null)
                values.Add(initial.Key, initial.Value);
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
