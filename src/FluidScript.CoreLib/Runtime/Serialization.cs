using System;
using System.Collections;
using System.Runtime.Serialization;

namespace FluidScript.Runtime
{
    /// <summary>
    /// Todo serialization support
    /// </summary>
    public static class Serialization
    {
        public static void Add(this SerializationInfo info, string name, object value, Type type)
        {
            if (type.IsPrimitive)
                info.AddValue(name, value, type);
            else if (value is IConvertible convertible)
            {
                info.AddValue(name, Convert.ChangeType(value, convertible.GetTypeCode()), type);
            }
            else
            {
                info.AddValue(name, value);
            }
        }

        public static void Serialize(this SerializationInfo info, IEnumerable enumerable)
        {
            var enumerator = enumerable.GetEnumerator();
            try
            {
                if (enumerator is IDictionaryEnumerator values)
                {
                    while (values.MoveNext())
                    {
                        var value = values.Entry.Value;
                        if (value is IConvertible)
                        {
                            value = Convert.ChangeType(value, ((IConvertible)value).GetTypeCode());
                        }
                        info.AddValue(Convert.ToString(values.Entry.Key, System.Globalization.CultureInfo.InvariantCulture), value);
                    }
                }
                else
                {
                    int capacity = 0;
                    if (enumerable is ICollection c)
                    {
                        capacity = c.Count;
                    }
                    ArrayList res = new ArrayList(capacity);
                    while (enumerator.MoveNext())
                    {
                        var item = enumerator.Current;
                        if (item is IConvertible)
                        {
                            res.Add(Convert.ChangeType(item, ((IConvertible)item).GetTypeCode()));
                        }
                        else
                        {
                            res.Add(item);
                        }
                    }
                    info.AddValue("values", res);
                }
            }
            finally
            {
                if (enumerator is IDisposable)
                    ((IDisposable)enumerator).Dispose();
            }
        }
    }
}
