using System.Dynamic;
using System.Linq.Expressions;

namespace FluidScript.Runtime
{
    internal sealed class MetaObject : DynamicMetaObject
    {
        private readonly IDynamicInvocable m_value;

        public MetaObject(Expression expression, IDynamicInvocable obj) : base(expression, BindingRestrictions.Empty, obj)
        {
            m_value = obj;
        }

        public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
        {
            if (m_value.TryGetBinder(binder.Name, out IMemberBinder member))
            {
                var value = member.Get(m_value);
                var expression = Expression.Convert(Expression.Constant(value, member.Type), binder.ReturnType);
                return new DynamicMetaObject(expression, GetTypeRestriction(this), value);
            }
            else
            {
                object value = null;
                var expression = Expression.Convert(Expression.Constant(value), binder.ReturnType);
                return new DynamicMetaObject(expression, GetTypeRestriction(this));
            }
        }

        public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject obj)
        {
            m_value.SafeSetValue(new Any(obj.Value, obj.LimitType), binder.Name);
            var expression = Expression.Convert(Expression.Constant(obj.Value, obj.LimitType), binder.ReturnType);
            return new DynamicMetaObject(expression, GetTypeRestriction(this));
        }

        public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args)
        {
            Any[] arguments = new Any[args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                arguments[i] = Any.op_Implicit(args[i].Value);
            }
            // todo check whether target is correct
            var expression = Expression.Convert(Expression.Constant(m_value.Invoke(binder.Name, arguments).m_value, binder.ReturnType), binder.ReturnType);
            return new DynamicMetaObject(expression, GetTypeRestriction(this));
        }

        internal static BindingRestrictions GetTypeRestriction(DynamicMetaObject obj)
        {
            if (obj.Value == null && obj.HasValue)
            {
                return BindingRestrictions.GetInstanceRestriction(obj.Expression, null);
            }
            else
            {
                return BindingRestrictions.GetTypeRestriction(obj.Expression, obj.LimitType);
            }
        }

        public override System.Collections.Generic.IEnumerable<string> GetDynamicMemberNames()
        {
            return m_value.Keys;
        }
    }
}
