using FluidScript.Extensions;
using FluidScript.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FluidScript.Runtime
{
    public
#if LATEST_VS
        readonly
#endif
        struct MethodInvoker
    {
        public readonly object Target;
        public readonly MethodInfo Method;
        public readonly ArgumentConversions Conversions;

        public MethodInvoker(object target, MethodInfo method, ArgumentConversions conversions)
        {
            Target = target;
            Method = method;
            Conversions = conversions;
        }

        public object Invoke(params object[] parameters)
        {
            if (Conversions != null && Conversions.Count > 0)
            {
                Conversions.Invoke(ref parameters);
            }
            return Method.Invoke(Target, parameters);
        }

        public static MethodInvoker Make<TDelegate>(Delegate value) where TDelegate : Delegate
        {
            var src = typeof(TDelegate).GetMethod(ReflectionUtils.InvokeMethod, ReflectionUtils.PublicInstance);
            if (ReflectionUtils.TryGetDelegateMethod(value.GetType(), src.GetParameters().Map(p => p.ParameterType), out MethodInfo dest, out ArgumentConversions conversions))
            {
                return new MethodInvoker(value, dest, conversions);
            }
            throw new Exception($"Cannot call the delegate of type {typeof(TDelegate)} with {value.GetType()}");
        }
    }
}
