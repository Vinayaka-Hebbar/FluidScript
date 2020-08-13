using System;

namespace FluidScript
{
    /// <summary>
    /// Converts a base data type to another base data type.
    /// </summary>
    [Runtime.Register(nameof(Convert))]
    public class FSConvert
    {
        [Runtime.Register("toBoolean")]
        public static Boolean ToBoolean(object value)
        {
            if (value == null)
                return Boolean.False;
            var c = value as IConvertible;
            return c == null || (c.GetTypeCode() != TypeCode.Boolean) || c.ToBoolean(null) ? Boolean.True : Boolean.False;
        }

        [Runtime.Register("toNumber")]
        public static Double ToNumber(object value)
        {
            if (!(value is IConvertible convertible))
                return new Double(0);
            return convertible.ToDouble(null);
        }

        [Runtime.Register("toString")]
        public static String ToString(object value)
        {
            return value == null ? new String(string.Empty) : new String(value.ToString());
        }

        [Runtime.Register("toAny")]
        public static object ToAny(object value)
        {
            switch (value)
            {
                case int i:
                    return new Integer(i);
                case sbyte b:
                    return new Byte(b);
                case short s:
                    return new Short(s);
                case long l:
                    return new Long(l);
                case float f:
                    return new Float(f);
                case double d:
                    return new Double(d);
                case bool b:
                    return new Boolean(b);
                case char c:
                    return new Char(c);
                case string s:
                    return new String(s);
                default:
                    return value;

            }
        }
    }
}
