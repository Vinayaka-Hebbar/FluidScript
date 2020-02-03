using System.Dynamic;
using System.Linq.Expressions;

namespace FluidScript.Dynamic
{
    internal sealed class MetaObject : DynamicMetaObject
    {
        private readonly LocalInstance m_value;

        public MetaObject(Expression expression, LocalInstance value) : base(expression, BindingRestrictions.Empty, value)
        {
            m_value = value;
        }

        public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
        {
            if (m_value.TryGetMember(binder.Name, out LocalVariable variable))
            {
                var value = m_value.Current.GetValue(variable);
                var expression = Expression.Constant(value, variable.Type);
                var result = Expression.Convert(expression, typeof(object));
                return new DynamicMetaObject(result, BindingRestrictions.GetTypeRestriction(result, variable.Type), value);
            }
            else
            {
                // todo null value should return
                object value = new object();
                var expression = Expression.Constant(value, typeof(object));
                return new DynamicMetaObject(expression, BindingRestrictions.GetTypeRestriction(expression, typeof(object)), value);
            }
        }

        public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
        {
            var result = value.Value;
            if (m_value.TryGetMember(binder.Name, out LocalVariable variable))
            {
                if (Reflection.TypeUtils.AreReferenceAssignable(variable.Type, value.LimitType))
                {
                    m_value.Current.Modify(variable, result);
                }
                else if (Reflection.TypeUtils.TryImplicitConvert(value.LimitType, variable.Type, out System.Reflection.MethodInfo implConvert))
                {
                    result = implConvert.Invoke(null, new object[1] { result });
                    m_value.Current.Modify(variable, result);
                }
                else
                {
                    throw new System.InvalidCastException(string.Concat(value.LimitType, " to ", variable.Type));
                }
            }
            else
            {
                // value not created
                variable = m_value.Create(binder.Name, result == null ? Reflection.TypeUtils.ObjectType : value.LimitType);
                m_value.Current[variable] = result;
            }
            var expression = Expression.Convert(Expression.Constant(result, result.GetType()), typeof(object));
            return new DynamicMetaObject(expression, BindingRestrictions.GetTypeRestriction(expression, result.GetType()), result);
        }

        public override System.Collections.Generic.IEnumerable<string> GetDynamicMemberNames()
        {
            return m_value.Keys;
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

        /// <summary>
        /// Returns a Restrictions object which includes our current restrictions merged
        /// with a restriction limiting our type
        /// </summary>
        private BindingRestrictions GetRestrictions()
        {
            return BindingRestrictions.GetTypeRestriction(Expression, null);
        }
    }
}
