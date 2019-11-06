using System.Linq;

namespace FluidScript
{
    public class RuntimeObject
    {
        private static readonly object Any = new object();
        public static readonly RuntimeObject Null = new RuntimeObject(Any, PrimitiveType.Any);
        public static readonly RuntimeObject Zero = new RuntimeObject(0, PrimitiveType.Double);
        public static readonly RuntimeObject True = new RuntimeObject(true, PrimitiveType.Bool);
        public static readonly RuntimeObject False = new RuntimeObject(false, PrimitiveType.Bool);
        public static readonly RuntimeObject NaN = new RuntimeObject(double.NaN, PrimitiveType.Double);
        public readonly object Value;
        public readonly PrimitiveType Type;

        public RuntimeObject(object value, PrimitiveType type)
        {
            Value = value;
            Type = type;
        }

        public RuntimeObject(RuntimeObject[] value)
        {
            Value = value;
            Type = PrimitiveType.Array;
        }

        public RuntimeObject(double value)
        {
            Value = value;
            Type = PrimitiveType.Double;
        }

        public RuntimeObject(float value)
        {
            Value = value;
            Type = PrimitiveType.Float;
        }

        public RuntimeObject(int value)
        {
            Value = value;
            Type = PrimitiveType.Int32;
        }

        public RuntimeObject(char value)
        {
            Value = value;
            Type = PrimitiveType.Char;
        }

        public RuntimeObject(string value)
        {
            Value = value;
            Type = PrimitiveType.String;
        }

        public RuntimeObject(bool value)
        {
            Value = value;
            Type = PrimitiveType.Bool;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public bool IsNumber() => (Type & PrimitiveType.Number) == PrimitiveType.Number;

        public bool IsString() => (Type & PrimitiveType.String) == PrimitiveType.String;

        public bool IsBool() => (Type & PrimitiveType.Bool) == PrimitiveType.Bool;

        public bool IsChar() => (Type & PrimitiveType.Char) == PrimitiveType.Char;

        public bool IsArray() => (Type & PrimitiveType.Array) == PrimitiveType.Array;

        public bool IsNull()
        {
            if ((Type & PrimitiveType.Number) == PrimitiveType.Number)
            {
                if ((Type & PrimitiveType.Double) == PrimitiveType.Double)
                    return double.IsNaN((double)Value);
                if ((Type & PrimitiveType.Float) == PrimitiveType.Float)
                    return float.IsNaN((float)Value);
            }
            return Value == null;
        }

        public double ToDouble()
        {
            if ((Type & PrimitiveType.Double) == PrimitiveType.Double)
                return (double)Value;
            if ((Type & PrimitiveType.Number) == PrimitiveType.Number)
                return System.Convert.ToDouble(Value);
            return double.NaN;
        }

        public float ToFloat()
        {
            if ((Type & PrimitiveType.Float) == PrimitiveType.Float)
                return (float)Value;
            if ((Type & PrimitiveType.Number) == PrimitiveType.Number)
                return System.Convert.ToSingle(Value);
            return float.NaN;
        }

        public int ToInt32()
        {
            if ((Type & PrimitiveType.Int32) == PrimitiveType.Int32)
                return (int)Value;
            if ((Type & PrimitiveType.Number) == PrimitiveType.Number)
                return System.Convert.ToInt32(Value);
            return 0;
        }

        public char ToChar()
        {
            if ((Type & PrimitiveType.Char) == PrimitiveType.Char)
                return (char)Value;
            return char.MinValue;
        }

        public bool ToBool()
        {
            if ((Type & PrimitiveType.Bool) == PrimitiveType.Bool)
                return (bool)Value;
            return false;
        }

        public double ToNumber()
        {
            //Check Cast
            if (Type == PrimitiveType.Double)
                return (double)Value;
            if (Type == PrimitiveType.Float)
                return (float)Value;
            if (Type == PrimitiveType.Int32)
                return (int)Value;
            if (Type == PrimitiveType.Bool)
                return (bool)Value ? 1 : 0;
            //force convert
            return System.Convert.ToDouble(Value);
        }

        public object[] ToArray()
        {
            if ((Type & PrimitiveType.Array) == PrimitiveType.Array)
            {
                //Check Cast
                if ((Type ^ PrimitiveType.Array) == PrimitiveType.Any)
                    return ((RuntimeObject[])Value).Select(obj => obj.Value).ToArray();
                return (object[])Value;
                //force convert
            }
            return new object[0];
        }

        public bool IsTypeOf<TSource>()
        {
            return Value != null && Value.GetType() == typeof(TSource);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return Value == null;
            }
            if (obj is RuntimeObject result)
            {
                return Value != null && Value.Equals(result.Value);
            }
            return Value != null && Value.Equals(obj);
        }

        public bool Equals(RuntimeObject other)
        {
            return Value != null && Value.Equals(other.Value);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public static implicit operator RuntimeObject(float value)
        {
            return new RuntimeObject(value);
        }

        public static implicit operator RuntimeObject(double value)
        {
            return new RuntimeObject(value);
        }

        public static implicit operator RuntimeObject(int value)
        {
            return new RuntimeObject(value);
        }

        public static implicit operator RuntimeObject(string value)
        {
            return new RuntimeObject(value);
        }

        public static implicit operator RuntimeObject(bool value)
        {
            return new RuntimeObject(value);
        }

        public static implicit operator RuntimeObject(char value)
        {
            return new RuntimeObject(value);
        }

        public static RuntimeObject operator +(RuntimeObject result1, RuntimeObject result2)
        {
            if (result1.IsNumber() && result2.IsNumber())
                return new RuntimeObject(result1.ToNumber() + result2.ToNumber());
            if (result1.IsString() || result2.IsString())
                return new RuntimeObject(result1.ToString() + result2.ToString());
            return Zero;
        }

        public static RuntimeObject operator -(RuntimeObject result1, RuntimeObject result2)
        {
            return new RuntimeObject(result1.ToNumber() - result2.ToNumber());
        }

        public static RuntimeObject operator *(RuntimeObject result1, RuntimeObject result2)
        {
            return new RuntimeObject(result1.ToNumber() * result2.ToNumber());
        }

        public static RuntimeObject operator /(RuntimeObject result1, RuntimeObject result2)
        {
            return new RuntimeObject(result1.ToNumber() / result2.ToNumber());
        }

        public static RuntimeObject operator %(RuntimeObject result1, RuntimeObject result2)
        {
            return new RuntimeObject(result1.ToNumber() % result2.ToNumber());
        }

        public static RuntimeObject operator &(RuntimeObject result1, RuntimeObject result2)
        {
            return new RuntimeObject((int)result1.Value & (int)result2.Value);
        }

        public static RuntimeObject operator |(RuntimeObject result1, RuntimeObject result2)
        {
            return new RuntimeObject((int)result1.Value | (int)result2.Value);
        }

        public static RuntimeObject operator ^(RuntimeObject result1, RuntimeObject result2)
        {
            return new RuntimeObject((int)result1.Value ^ (int)result2.Value);
        }

        public static RuntimeObject operator ~(RuntimeObject result1)
        {
            result1++;
            return new RuntimeObject((int)result1.Value);
        }

        public static RuntimeObject operator ++(RuntimeObject result1)
        {
            double value = result1.ToNumber();
            value++;
            return new RuntimeObject(value);
        }

        public static RuntimeObject operator --(RuntimeObject result1)
        {
            double value = result1.ToNumber();
            value--;
            return new RuntimeObject(value);
        }

        public static RuntimeObject operator >(RuntimeObject result1, RuntimeObject result2)
        {
            return new RuntimeObject(result1.ToNumber() > result2.ToNumber());
        }

        public static RuntimeObject operator >=(RuntimeObject result1, RuntimeObject result2)
        {
            return new RuntimeObject(result1.ToNumber() >= result2.ToNumber());
        }

        public static RuntimeObject operator <(RuntimeObject result1, RuntimeObject result2)
        {
            return new RuntimeObject(result1.ToNumber() < result2.ToNumber());
        }

        public static RuntimeObject operator <=(RuntimeObject result1, RuntimeObject result2)
        {
            return new RuntimeObject(result1.ToNumber() <= result2.ToNumber());
        }

        public static RuntimeObject operator ==(RuntimeObject result1, RuntimeObject result2)
        {
            return new RuntimeObject(result1.Equals(result2.Value));
        }

        public static RuntimeObject operator !=(RuntimeObject result1, RuntimeObject result2)
        {
            return new RuntimeObject(!result1.Equals(result2.Value));
        }

        public static RuntimeObject operator !(RuntimeObject result1)
        {
            if (result1.IsBool())
                return new RuntimeObject(!result1.ToBool());
            return Zero;
        }

        public static RuntimeObject operator +(RuntimeObject result1, int value)
        {
            if (result1.IsNumber())
                return new RuntimeObject(result1.ToNumber() + value);
            return new RuntimeObject(value);
        }

        public static RuntimeObject operator +(RuntimeObject result1)
        {
            if (result1.IsNumber())
                return new RuntimeObject(+result1.ToNumber());
            return Zero;
        }

        public static RuntimeObject operator -(RuntimeObject result1, int value)
        {
            return new RuntimeObject(result1.ToNumber() - value);
        }

        public static RuntimeObject operator -(RuntimeObject result1)
        {
            if (result1.IsNumber())
                return new RuntimeObject(-result1.ToNumber());
            return Zero;
        }

        public static RuntimeObject operator <<(RuntimeObject result1, int value)
        {
            return new RuntimeObject((int)result1.Value << value);
        }

        public static RuntimeObject operator >>(RuntimeObject result1, int value)
        {
            return new RuntimeObject((int)result1.Value >> value);
        }

        public static explicit operator int(RuntimeObject result)
        {
            return (int)result.Value;
        }

        public static explicit operator float(RuntimeObject result)
        {
            return (float)result.Value;
        }

        public static explicit operator double(RuntimeObject result)
        {
            return (double)result.Value;
        }

        public static explicit operator string(RuntimeObject result)
        {
            return result.Value.ToString();
        }

        public static explicit operator char(RuntimeObject result)
        {
            return (char)result.Value;
        }

        public static explicit operator bool(RuntimeObject result)
        {
            return (bool)result.Value;
        }
    }
}
