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
        internal readonly object Store;
        public readonly PrimitiveType Type;

        internal bool IsReturn;

        public RuntimeObject(object value, PrimitiveType type)
        {
            Store = value;
            Type = type;
        }

        public RuntimeObject(RuntimeObject[] value)
        {
            Store = value;
            Type = PrimitiveType.Array;
        }

        public RuntimeObject(double value)
        {
            Store = value;
            Type = PrimitiveType.Double;
        }

        public RuntimeObject(float value)
        {
            Store = value;
            Type = PrimitiveType.Float;
        }

        public RuntimeObject(int value)
        {
            Store = value;
            Type = PrimitiveType.Int32;
        }

        public RuntimeObject(char value)
        {
            Store = value;
            Type = PrimitiveType.Char;
        }

        public RuntimeObject(string value)
        {
            Store = value;
            Type = PrimitiveType.String;
        }

        public RuntimeObject(bool value)
        {
            Store = value;
            Type = PrimitiveType.Bool;
        }

        public override string ToString()
        {
            if((Type & PrimitiveType.Array) == PrimitiveType.Array)
            {
                return string.Concat("[",string.Join(",", ((RuntimeObject[])Store).Select(value=>value.ToString())),"]");
            }
            return Store.ToString();
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
                    return double.IsNaN((double)Store);
                if ((Type & PrimitiveType.Float) == PrimitiveType.Float)
                    return float.IsNaN((float)Store);
            }
            return Store == Any;
        }

        public double ToDouble()
        {
            if ((Type & PrimitiveType.Double) == PrimitiveType.Double)
                return System.Convert.ToDouble(Store);
            return double.NaN;
        }

        public float ToFloat()
        {
            if ((Type & PrimitiveType.Float) == PrimitiveType.Float || (Type & PrimitiveType.Number) == PrimitiveType.Number)
                return System.Convert.ToSingle(Store);
            return float.NaN;
        }

        public int ToInt32()
        {
            if ((Type & PrimitiveType.Int32) == PrimitiveType.Int32 || (Type & PrimitiveType.Number) == PrimitiveType.Number)
                return System.Convert.ToInt32(Store);
            return 0;
        }

        public char ToChar()
        {
            if ((Type & PrimitiveType.Char) == PrimitiveType.Char)
                return System.Convert.ToChar(Store);
            return char.MinValue;
        }

        public bool ToBool()
        {
            if ((Type & PrimitiveType.Bool) == PrimitiveType.Bool)
                return (bool)Store;
            return false;
        }

        public double ToNumber()
        {
            //Check Cast
            if (Type == PrimitiveType.Double)
                return System.Convert.ToDouble(Store);
            if (Type == PrimitiveType.Float)
                return System.Convert.ToSingle(Store);
            if (Type == PrimitiveType.Int32)
                return System.Convert.ToInt32(Store);
            if (Type == PrimitiveType.Bool)
                return (bool)Store ? 1 : 0;
            //force convert
            return System.Convert.ToDouble(Store);
        }

        public RuntimeObject[] ToArray()
        {
            if ((Type & PrimitiveType.Array) == PrimitiveType.Array)
            {
                return (RuntimeObject[])Store;
                //force convert
            }
            return new RuntimeObject[0];
        }

        public bool IsTypeOf<TSource>()
        {
            return Store != null && Store.GetType() == typeof(TSource);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return Store == null;
            }
            if (obj is RuntimeObject result)
            {
                return Store != null && Store.Equals(result.Store);
            }
            return Store != null && Store.Equals(obj);
        }

        public bool Equals(RuntimeObject other)
        {
            return Store != null && Store.Equals(other.Store);
        }

        public override int GetHashCode()
        {
            return Store.GetHashCode();
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
            return new RuntimeObject((int)result1.Store & (int)result2.Store);
        }

        public static RuntimeObject operator |(RuntimeObject result1, RuntimeObject result2)
        {
            return new RuntimeObject((int)result1.Store | (int)result2.Store);
        }

        public static RuntimeObject operator ^(RuntimeObject result1, RuntimeObject result2)
        {
            return new RuntimeObject((int)result1.Store ^ (int)result2.Store);
        }

        public static RuntimeObject operator ~(RuntimeObject result1)
        {
            result1++;
            return new RuntimeObject((int)result1.Store);
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
            return new RuntimeObject(result1.Equals(result2.Store));
        }

        public static RuntimeObject operator !=(RuntimeObject result1, RuntimeObject result2)
        {
            return new RuntimeObject(!result1.Equals(result2.Store));
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
            return new RuntimeObject((int)result1.Store << value);
        }

        public static RuntimeObject operator >>(RuntimeObject result1, int value)
        {
            return new RuntimeObject((int)result1.Store >> value);
        }

        public static explicit operator int(RuntimeObject result)
        {
            return (int)result.Store;
        }

        public static explicit operator float(RuntimeObject result)
        {
            return (float)result.Store;
        }

        public static explicit operator double(RuntimeObject result)
        {
            return (double)result.Store;
        }

        public static explicit operator string(RuntimeObject result)
        {
            return result.Store.ToString();
        }

        public static explicit operator char(RuntimeObject result)
        {
            return (char)result.Store;
        }

        public static explicit operator bool(RuntimeObject result)
        {
            return (bool)result.Store;
        }
    }
}
