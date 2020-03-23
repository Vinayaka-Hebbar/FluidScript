namespace FluidScript.Runtime
{
    public class MetaObjectProvider
    {
        private readonly DynamicObject m_value;

        internal MetaObjectProvider(DynamicObject value)
        {
            m_value = value;
        }

        public virtual Compiler.Binders.IBinder BindGetMember(string name)
        {
            if (m_value.TryGetMember(name, out LocalVariable variable))
            {
                return new Compiler.Binders.DynamicVariableBinder(variable, m_value);
            }
            return null;
        }

        public virtual TypedValue? BindSetMember(string name, System.Type type, object value)
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

        internal System.Delegate GetDelegate(string name, object[] args, out Compiler.Binders.ArgumentConversions binders)
        {
            System.Reflection.MethodInfo method = null;
            object obj = null;
            binders = new Compiler.Binders.ArgumentConversions(args.Length);
            if (m_value.TryGetMember(name, out LocalVariable variable))
            {
                if (m_value[variable.Index] is System.Delegate refer)
                {
                    System.Reflection.MethodInfo m = refer.Method;
                    // only static method can allowed
                    if (Utils.TypeHelpers.MatchesTypes(m, args, binders))
                    {
                        method = m;
                        obj = refer.Target;
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
