using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;

namespace FluidScript.Dynamic
{
    internal sealed class MetaObject : DynamicMetaObject
    {
        private readonly LocalScope m_value;

        public MetaObject(Expression expression, BindingRestrictions restrictions, LocalScope value) : base(expression, restrictions, value)
        {
            m_value = value;
        }



        public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
        {
            object res = m_value[binder.Name];
            return new DynamicMetaObject(Expression.Constant(res, typeof(object)), GetBindingRestrictions());
        }

        public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
        {
            m_value.CreateOrModify(binder.Name, value.Value);
            return new DynamicMetaObject(Expression.Convert(value.Expression, typeof(object)), GetBindingRestrictions());
        }

        public override System.Collections.Generic.IEnumerable<string> GetDynamicMemberNames()
        {
            return m_value.Select(val => val.Name);
        }

        private BindingRestrictions GetBindingRestrictions()
        {
            if (Value == null && HasValue)
            {
                return BindingRestrictions.GetInstanceRestriction(Expression, null);
            }
            else
            {
                return BindingRestrictions.GetTypeRestriction(Expression, LimitType);
            }
        }
    }
}
