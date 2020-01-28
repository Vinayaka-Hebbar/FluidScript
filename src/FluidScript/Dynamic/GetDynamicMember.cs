using System.Dynamic;

namespace FluidScript.Dynamic
{
    internal sealed class GetDynamicMember : System.Dynamic.GetMemberBinder
    {
        public GetDynamicMember(string name, bool ignoreCase) : base(name, ignoreCase)
        {
        }

        public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
        {
            return target;
        }
    }
}
