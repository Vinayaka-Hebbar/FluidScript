using FluidScript.Reflection;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace FluidScript.Compiler.Metadata
{
    public class ModulePrototype : Prototype
    {
        public ModulePrototype(Prototype parent, string name, ScopeContext context) : base(parent, name, context)
        {
        }

        public override RuntimeObject CreateInstance()
        {
            throw new System.Exception("Instance of module can't be created");
        }

        public override void DefineVariable(string name, RuntimeObject value)
        {
            throw new System.Exception("Not Supported");
        }

        public override IEnumerable<DeclaredField> GetFields()
        {
            throw new System.Exception("No Fields can be inside module");
        }

        public override IEnumerable<DeclaredMember> GetMembers()
        {
            throw new System.NotImplementedException();
        }

        public override IEnumerable<DeclaredMethod> GetMethods()
        {
            throw new System.NotImplementedException();
        }

        public override bool HasMember(string name)
        {
            return false;
        }

        internal override Instances Init(RuntimeObject instance, [Optional] KeyValuePair<object, RuntimeObject> initial)
        {
            throw new System.NotImplementedException();
        }

        internal override Prototype Merge(Prototype prototype2)
        {
            throw new System.NotImplementedException();
        }
    }
}
