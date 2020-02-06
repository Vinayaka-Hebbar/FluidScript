using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;

namespace FluidScript.Dynamic
{
    internal sealed class MetaObject : DynamicMetaObject
    {
        private readonly RuntimeMetaObject m_value;

        public MetaObject(Expression expression, IRuntimeMetaObjectProvider runtime) : base(expression, BindingRestrictions.Empty, runtime)
        {
            m_value = runtime.GetMetaObject();
        }

        public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
        {
            var result = m_value.BindGetMember(binder.Name);
            var value = result.Value;
            if (value != null)
            {
                var expression = Expression.Constant(value, value.GetType());
                var conv = Expression.Convert(expression, typeof(object));
                return new DynamicMetaObject(conv, BindingRestrictions.GetTypeRestriction(conv, result.Type), value);
            }
            else
            {
                var expression = Expression.Constant(value, result.Type);
                return new DynamicMetaObject(expression, BindingRestrictions.GetTypeRestriction(expression, result.Type), value);
            }
        }

        public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
        {
            var result = m_value.BindSetMember(binder.Name, value.LimitType, value.Value);
            var expression = Expression.Convert(Expression.Constant(result, value.LimitType), value.LimitType);
            return new DynamicMetaObject(expression, BindingRestrictions.GetTypeRestriction(expression, value.LimitType), result);
        }

        public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args)
        {
            var name = binder.Name;
            var arguments = args.Select(arg => arg.Value).ToArray();
            var result = m_value.BindInvokeMemeber(name, arguments);
            var expression = Expression.Constant(result);
            return new DynamicMetaObject(expression, BindingRestrictions.Empty, result);
        }

        public override System.Collections.Generic.IEnumerable<string> GetDynamicMemberNames()
        {
            return m_value.Keys;
        }
    }
}
