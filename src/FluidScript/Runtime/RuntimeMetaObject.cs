namespace FluidScript.Runtime
{
    public sealed class RuntimeMetaObject
    {
        private readonly DynamicObject m_value;

        internal RuntimeMetaObject(DynamicObject value)
        {
            m_value = value;
        }

        internal TypedValue? BindGetMember(string name)
        {
            if (m_value.TryGetMember(name, out LocalVariable variable))
            {
                var value = m_value[variable.Index];
                return new TypedValue(value, variable.Type);
            }
            else
            {
                return null;
            }
        }

        internal TypedValue? BindSetMember(string name, System.Type type, object value)
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
                return new TypedValue(value, variable.Type);
            }
            else
            {
                // value not created
                m_value.Add(name, type, value);
                return new TypedValue(value, type);
            }
        }

        internal System.Delegate GetDelegate(string name, object[] args, out Compiler.Binders.ArgumentBinderList binds)
        {
            System.Reflection.MethodInfo method = null;
            object obj = null;
            binds = new Compiler.Binders.ArgumentBinderList();
            if (m_value.TryGetMember(name, out LocalVariable variable))
            {
                if (m_value[variable.Index] is System.Delegate refer)
                {
                    System.Reflection.MethodInfo m = refer.Method;
                    // only static method can allowed
                    if (refer.Target is Function function)
                    {
                        if (Utils.TypeHelpers.MatchesTypes(function.ParameterTypes, args, ref binds))
                        {
                            args = new object[] { args };
                            obj = function;
                            method = m;
                        }
                    }
                    else
                    {
                        if (Utils.TypeHelpers.MatchesTypes(m, args, ref binds))
                        {
                            method = m;
                            obj = refer.Target;
                        }
                    }
                    return refer;
                }
            }
            return new System.Func<object>(() => null);

        }

        internal System.Collections.Generic.IEnumerable<string> Keys
        {
            get => m_value.Keys;
        }
    }
}
