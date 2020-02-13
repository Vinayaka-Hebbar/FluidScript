using System;

namespace FluidScript
{
    public static class FSConvert
    {
        [Runtime.Register("toBoolean")]
        public static Boolean ToBoolean(object value)
        {
            return Convert.ToBoolean(value) ? Boolean.True : Boolean.False;
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
