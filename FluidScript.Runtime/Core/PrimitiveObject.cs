using System;
using System.Linq;

namespace FluidScript.Core
{
    public sealed class PrimitiveObject : RuntimeObject
    {
        private readonly object Store;
        private readonly PrimitiveType Type;

        public PrimitiveObject(object value, PrimitiveType type)
        {
            Store = value;
            Type = type;
        }

        internal PrimitiveObject(object value, Type type)
        {
            Store = value;
            Type = Compiler.Emit.TypeUtils.ToPrimitive(type);
        }

        public PrimitiveObject(PrimitiveObject[] value)
        {
            Store = value;
            Type = PrimitiveType.Array;
        }

        public PrimitiveObject(double value)
        {
            Store = value;
            Type = PrimitiveType.Double;
        }

        public PrimitiveObject(float value)
        {
            Store = value;
            Type = PrimitiveType.Float;
        }

        public PrimitiveObject(int value)
        {
            Store = value;
            Type = PrimitiveType.Int32;
        }

        public PrimitiveObject(char value)
        {
            Store = value;
            Type = PrimitiveType.Char;
        }

        public PrimitiveObject(string value)
        {
            Store = value;
            Type = PrimitiveType.String;
        }

        public PrimitiveObject(bool value)
        {
            Store = value;
            Type = PrimitiveType.Bool;
        }

        public override string ToString()
        {
            return Store.ToString();
        }

        public override bool IsNumber() => (Type & PrimitiveType.Number) == PrimitiveType.Number;

        public override bool IsString() => (Type & PrimitiveType.String) == PrimitiveType.String;

        public override bool IsBool() => (Type & PrimitiveType.Bool) == PrimitiveType.Bool;

        public override bool IsChar() => (Type & PrimitiveType.Char) == PrimitiveType.Char;

        public override bool IsArray() => (Type & PrimitiveType.Array) == PrimitiveType.Array;

        public override bool IsNull()
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

        public override double ToDouble()
        {
            if ((Type & PrimitiveType.Double) == PrimitiveType.Double)
                return System.Convert.ToDouble(Store);
            return double.NaN;
        }

        public override float ToFloat()
        {
            if ((Type & PrimitiveType.Float) == PrimitiveType.Float || (Type & PrimitiveType.Number) == PrimitiveType.Number)
                return System.Convert.ToSingle(Store);
            return float.NaN;
        }

        public override int ToInt32()
        {
            if ((Type & PrimitiveType.Int32) == PrimitiveType.Int32 || (Type & PrimitiveType.Number) == PrimitiveType.Number)
                return System.Convert.ToInt32(Store);
            return 0;
        }

        public override char ToChar()
        {
            if ((Type & PrimitiveType.Char) == PrimitiveType.Char)
                return System.Convert.ToChar(Store);
            return char.MinValue;
        }

        public override bool ToBool()
        {
            if ((Type & PrimitiveType.Bool) == PrimitiveType.Bool)
                return (bool)Store;
            return false;
        }

        public override double ToNumber()
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
        
        public PrimitiveObject[] ToArray()
        {
            if ((Type & PrimitiveType.Array) == PrimitiveType.Array)
            {
                return (PrimitiveObject[])Store;
                //force convert
            }
            return new PrimitiveObject[0];
        }

        public bool IsTypeOf<TSource>()
        {
            return Store != null && Store.GetType() == typeof(TSource);
        }

        public override Type DeclaredType => Store.GetType();

        public override object Instance()
        {
            return Store;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return Store == null;
            }
            if (obj is PrimitiveObject result)
            {
                return Store != null && Store.Equals(result.Store);
            }
            return Store != null && Store.Equals(obj);
        }

        public bool Equals(PrimitiveObject other)
        {
            return Store != null && Store.Equals(other.Store);
        }

        public override int GetHashCode()
        {
            return Store.GetHashCode();
        }

        public override PrimitiveType RuntimeType => Type;


        public static explicit operator int(PrimitiveObject result)
        {
            return (int)result.Store;
        }

        public static implicit operator float(PrimitiveObject result)
        {
            return (float)result.Store;
        }

        public static implicit operator double(PrimitiveObject result)
        {
            return (double)result.Store;
        }

        public static implicit operator string(PrimitiveObject result)
        {
            return result.Store.ToString();
        }

        public static implicit operator char(PrimitiveObject result)
        {
            return (char)result.Store;
        }

        public static implicit operator bool(PrimitiveObject result)
        {
            return (bool)result.Store;
        }
    }
}
