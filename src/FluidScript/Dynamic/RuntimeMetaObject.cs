

using System.Linq;

namespace FluidScript.Dynamic
{
    public sealed class RuntimeMetaObject
    {
        private readonly DynamicObject m_value;

        internal RuntimeMetaObject(DynamicObject value)
        {
            m_value = value;
        }

        internal MetaResult BindGetMember(string name)
        {
            if (m_value.TryGetMember(name, out LocalVariable variable))
            {
                var value = m_value.GetValue(variable);
                return new MetaResult(value, variable.Type);
            }
            else
            {
                return MetaResult.Empty;
            }
        }

        internal object BindSetMember(string name, System.Type type, object value)
        {
            if (m_value.TryGetMember(name, out LocalVariable variable))
            {
                if (Utils.TypeUtils.AreReferenceAssignable(variable.Type, type))
                {
                    m_value.Modify(variable, value);
                }
                else if (Utils.TypeUtils.TryImplicitConvert(type, variable.Type, out System.Reflection.MethodInfo implConvert))
                {
                    value = implConvert.Invoke(null, new object[1] { value });
                    m_value.Modify(variable, value);
                }
                else
                {
                    throw new System.InvalidCastException(string.Concat(type, " to ", variable.Type));
                }
            }
            else
            {
                // value not created
                m_value.Add(name, type, value);
            }
            return value;
        }

        internal object BindInvokeMemeber(string name, object[] args)
        {
            var variables = m_value.FindValues(name, (item) => typeof(System.Delegate).IsAssignableFrom(item.Type)).ToArray();
            if (variables.Length > 0)
            {
                object obj = null;
                var bindings = new Reflection.Emit.ParamBindList();
                System.Reflection.MethodInfo method = null;
                for (int index = 0; index < variables.Length; index++)
                {
                    var refer = (System.Delegate)variables[index];
                    System.Reflection.MethodInfo m = refer.Method;
                    // only static method can allowed
                    if (refer.Target is System.Reflection.MethodInfo)
                    {
                        m = (System.Reflection.MethodInfo)refer.Target;
                        if (Utils.TypeUtils.MatchesTypes(m, args, ref bindings))
                        {
                            method = m;
                            break;
                        }
                    }
                    else if (refer.Target is Function function)
                    {
                        if (Utils.TypeUtils.MatchesTypes(function.ParameterTypes, args, ref bindings))
                        {
                            args = new object[] { args };
                            obj = function;
                            method = m;
                            break;
                        }
                    }
                }

                foreach (var binding in bindings)
                {
                    if (binding.BindType == Reflection.Emit.ParamBind.ParamBindType.Convert)
                    {
                        args[binding.Index] = binding.Invoke(args);
                    }
                    else if (binding.BindType == Reflection.Emit.ParamBind.ParamBindType.ParamArray)
                    {
                        args = (object[])binding.Invoke(args);
                        break;
                    }
                }
                return method.Invoke(obj, args);
            }
            return null;
        }

        internal System.Collections.Generic.IEnumerable<string> Keys
        {
            get => m_value.Keys;
        }
    }

    internal
#if LATEST_VS
        readonly
#endif
        struct MetaResult
    {
        internal static readonly MetaResult Empty = new MetaResult(null, typeof(object));
        internal readonly object Value;
        internal readonly System.Type Type;

        public MetaResult(object value, System.Type type)
        {
            Value = value;
            Type = type;
        }
    }
}
