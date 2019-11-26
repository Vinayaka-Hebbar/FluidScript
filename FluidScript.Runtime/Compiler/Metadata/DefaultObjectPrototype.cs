using FluidScript.Compiler.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace FluidScript.Compiler.Metadata
{
    internal sealed class DefaultObjectPrototype : Prototype
    {
        public readonly ICollection<DeclaredMember> members;
        public DefaultObjectPrototype(IEnumerable<DeclaredMember> members) : base(null, "RuntimeObject", ScopeContext.Type)
        {
            this.members = new List<DeclaredMember>(members);
        }

        public override bool HasMember(string name)
        {
            return false;
        }

        public override IEnumerable<DeclaredMethod> GetMethods()
        {
            return members.OfType<DeclaredMethod>();
        }

        public override IEnumerable<DeclaredField> GetFields()
        {
            return Enumerable.Empty<DeclaredField>();
        }

        public override IEnumerable<DeclaredMember> GetMembers()
        {
            return members;
        }

#if Runtime
        public override RuntimeObject CreateInstance()
        {
            return new RuntimeObject(this);
        }

        public override void DefineVariable(string name, RuntimeObject value)
        {
            throw new System.Exception("can't define variable");
        }

        internal override Instances Init(RuntimeObject obj, [Optional] KeyValuePair<object, RuntimeObject> initial)
        {
            var instance = new Instances();
            if (initial.Key != null)
                instance.Add(initial.Key, initial.Value);
            foreach (DeclaredMethod method in members)
            {
                instance.AttachFunction(obj, method);
            }
            return instance;
        }

        internal override Prototype Merge(Prototype prototype2)
        {
            var members = Enumerable.Concat(GetMembers(), prototype2.GetMembers());
            return new ObjectPrototype(members, null, this, string.Concat(Name, "_", prototype2.Name));
        }
#endif
    }
}
