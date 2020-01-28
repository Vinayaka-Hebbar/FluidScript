using System.Dynamic;

namespace FluidScript.Dynamic
{
    internal class SetDynamicMember : System.Dynamic.SetMemberBinder
    {
        public SetDynamicMember(string name, bool ignoreCase) : base(name, ignoreCase)
        {
        }

        public override DynamicMetaObject FallbackSetMember(DynamicMetaObject target, DynamicMetaObject value, DynamicMetaObject errorSuggestion)
        {
            return value;
        }
    }
}
