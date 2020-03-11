using System.Dynamic;
using System.Linq.Expressions;

namespace FluidScript.Runtime
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
            if (result != null)
            {
                var value = result.Get(Value);
                var expression = Expression.Convert(Expression.Constant(value, result.Type), binder.ReturnType);
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
            m_value.BindSetMember(binder.Name, obj.LimitType, obj.Value);
            var expression = Expression.Convert(Expression.Constant(obj.Value, obj.LimitType), binder.ReturnType);
            return new DynamicMetaObject(expression, GetTypeRestriction(this));
        }

        public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args)
        {
            var name = binder.Name;
            var arguments = Utils.CollectionExtensions.Map(args, arg => arg.Value);
            var del = m_value.GetDelegate(name, arguments, out Compiler.Binders.ArgumenConversions conversions);
            var method = del.Method;
            // todo check whether target is correct
            foreach (var conversion in conversions)
            {
                if (conversion.ConversionType == Compiler.Binders.ConversionType.Convert)
                {
                    arguments[conversion.Index] = conversion.Invoke(arguments);
                }
                else if (conversion.ConversionType == Compiler.Binders.ConversionType.ParamArray)
                {
                    arguments = (object[])conversion.Invoke(arguments);
                    break;
                }
            }
            var result = method.Invoke(del.Target, arguments);
            var expression = Expression.Convert(Expression.Constant(result, method.ReturnType), binder.ReturnType);
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
