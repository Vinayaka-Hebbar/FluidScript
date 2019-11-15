using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;

namespace FluidScript.Compiler.Metadata
{
#if Runtime
    internal class MetaObject : DynamicMetaObject
    {
        public MetaObject(Expression expression, object value) : base(expression, BindingRestrictions.Empty, value)
        {
        }

        public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args)
        {
            var parameters = args.Select(arg => RuntimeObject.From(arg.Value)).ToArray();
            if (Value is RuntimeObject runtime)
            {
                return new DynamicMetaObject(Expression.Constant(runtime.Call(binder.Name, parameters)), BindingRestrictions.GetTypeRestriction(Expression, LimitType));
            }
            return new DynamicMetaObject(Expression.Empty(), BindingRestrictions.Empty);
        }

        public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
        {
            if (Value is RuntimeObject runtime)
            {
                return new DynamicMetaObject(Expression.Constant(runtime[binder.Name]), BindingRestrictions.GetTypeRestriction(Expression, LimitType));
            }
            return new DynamicMetaObject(Expression.Empty(), BindingRestrictions.Empty);
        }

        public override DynamicMetaObject BindInvoke(InvokeBinder binder, DynamicMetaObject[] args)
        {
            var parameters = args.Select(arg => RuntimeObject.From(arg.Value)).ToArray();
            if (Value is RuntimeObject runtime)
            {
                return new DynamicMetaObject(Expression.Constant(runtime.DynamicInvoke(parameters)), BindingRestrictions.GetTypeRestriction(Expression, LimitType));
            }
            return new DynamicMetaObject(Expression.Empty(), BindingRestrictions.Empty);
        }

        public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
        {
            if (Value is RuntimeObject runtime)
            {
                runtime[binder.Name] = RuntimeObject.From(value.Value);
                return new DynamicMetaObject(Expression.Call(GetType().GetMethod("Return")), BindingRestrictions.GetTypeRestriction(Expression, LimitType));
            }
            return new DynamicMetaObject(Expression.Empty(), BindingRestrictions.Empty);
        }

        public static object Return()
        {
            return string.Empty;
        }
    }
#endif
}
