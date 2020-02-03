using System.Runtime.Serialization;

namespace FluidScript.Runtime
{
    /// <summary>
    /// Todo serialization support
    /// </summary>
    public static class Serialization
    {
        public static void Add(this SerializationInfo info, string name, object value, System.Type type)
        {
            if (type.IsPrimitive)
                info.AddValue(name, value, type);
            else if (value is System.IConvertible convertible)
            {
                info.AddValue(name, System.Convert.ChangeType(value, convertible.GetTypeCode()), type);
            }
            else if (value is String)
            {
                info.AddValue(name, value.ToString(), typeof(string));
            }
            else
            {
                info.AddValue(name, value);
            }
        }
    }
}
