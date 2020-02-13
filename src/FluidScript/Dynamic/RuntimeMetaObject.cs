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
                var value = m_value.Data[variable.Index];
                return new MetaResult(value, variable.Type);
            }
            else
            {
                return MetaResult.Empty;
            }
        }

        internal object BindSetMember(string name, System.Type type, object value)
        {
            // handle null type
            if (m_value.TryGetMember(name, out LocalVariable variable))
            {
                System.Type dest = variable.Type;
                if (type == null)
                {
                    if (Utils.TypeUtils.IsNullAssignable(dest))
                        m_value.Update(variable, value);
                    else
                        throw new System.Exception(string.Concat("Can't assign null value to type ", dest));
                }
                else if (Utils.TypeUtils.AreReferenceAssignable(dest, type))
                {
                    m_value.Update(variable, value);
                }
                else if (Utils.TypeUtils.TryImplicitConvert(type, dest, out System.Reflection.MethodInfo implConvert))
                {
                    value = implConvert.Invoke(null, new object[1] { value });
                    m_value.Update(variable, value);
                }
                else
                {
                    throw new System.InvalidCastException(string.Concat(type, " to ", dest));
                }
            }
            else
            {
                // value not created
                m_value.Insert(name, type, value, true);
            }
            return value;
        }

        internal object BindInvokeMemeber(string name, object[] args)
        {
            object obj = null;
            if (m_value.TryGetMember(name, out LocalVariable variable))
            {
                var bindings = new Reflection.Emit.ParamBindList();
                System.Reflection.MethodInfo method = null;
                var refer = (System.Delegate)m_value.Data[variable.Index];
                System.Reflection.MethodInfo m = refer.Method;
                // only static method can allowed
                if (refer.Target is Function function)
                {
                    if (Utils.DynamicUtils.MatchesTypes(function.ParameterTypes, args, ref bindings))
                    {
                        args = new object[] { args };
                        obj = function;
                        method = m;
                    }
                }
                else
                {
                    m = refer.Method;
                    if (Utils.DynamicUtils.MatchesTypes(m, args, ref bindings))
                    {
                        method = m;
                        obj = refer.Target;
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
            return obj;

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
