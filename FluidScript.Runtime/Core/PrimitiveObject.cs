using System;
using FluidScript.Compiler.Metadata;

namespace FluidScript.Core
{
#if Runtime
    public sealed class PrimitiveObject : ObjectInstance
    {
        private static Compiler.Metadata.Prototype prototype;

        private readonly object Store;
        private readonly RuntimeType Type;

        public PrimitiveObject(object value, RuntimeType type)
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
            Type = RuntimeType.Array;
        }

        public PrimitiveObject(double value)
        {
            Store = value;
            Type = RuntimeType.Double;
        }

        public PrimitiveObject(float value)
        {
            Store = value;
            Type = RuntimeType.Float;
        }

        public PrimitiveObject(int value)
        {
            Store = value;
            Type = RuntimeType.Int32;
        }

        public PrimitiveObject(char value)
        {
            Store = value;
            Type = RuntimeType.Char;
        }

        public PrimitiveObject(string value)
        {
            Store = value;
            Type = RuntimeType.String;
        }

        public PrimitiveObject(bool value)
        {
            Store = value;
            Type = RuntimeType.Bool;
        }

        public override string ToString()
        {
            return Store.ToString();
        }

        public override bool IsNumber() => (Type & RuntimeType.Double) == RuntimeType.Double;

        public override bool IsString() => (Type & RuntimeType.String) == RuntimeType.String;

        public override bool IsBool() => (Type & RuntimeType.Bool) == RuntimeType.Bool;

        public override bool IsChar() => (Type & RuntimeType.Char) == RuntimeType.Char;

        public override bool IsArray() => (Type & RuntimeType.Array) == RuntimeType.Array;

        public override bool IsNull()
        {
            if ((Type & RuntimeType.Double) == RuntimeType.Double)
            {
                return false;
            }
            return Store == _null || Store == _undefined;
        }

        public override double ToDouble()
        {
            if ((Type & RuntimeType.Double) == RuntimeType.Double)
                return System.Convert.ToDouble(Store);
            return double.NaN;
        }

        public override float ToFloat()
        {
            if ((Type & RuntimeType.Float) == RuntimeType.Float)
                return System.Convert.ToSingle(Store);
            return float.NaN;
        }

        public override int ToInt32()
        {
            if ((Type & RuntimeType.Int32) == RuntimeType.Int32)
                return System.Convert.ToInt32(Store);
            return 0;
        }

        public override char ToChar()
        {
            if ((Type & RuntimeType.Char) == RuntimeType.Char)
                return System.Convert.ToChar(Store);
            return char.MinValue;
        }

        public override bool ToBool()
        {
            if ((Type & RuntimeType.Bool) == RuntimeType.Bool)
                return System.Convert.ToBoolean(Store);
            return Type != RuntimeType.Undefined;
        }

        public override double ToNumber()
        {
            //Check Cast
            if (Type == RuntimeType.Double)
                return System.Convert.ToDouble(Store);
            if (Type == RuntimeType.Float)
                return System.Convert.ToSingle(Store);
            if (Type == RuntimeType.Int32)
                return System.Convert.ToInt32(Store);
            if (Type == RuntimeType.Bool)
                return (bool)Store ? 1 : 0;
            //force convert
            return System.Convert.ToDouble(Store);
        }

        public PrimitiveObject[] ToArray()
        {
            if ((Type & RuntimeType.Array) == RuntimeType.Array)
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

        public override RuntimeType ReflectedType => Type;


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

        public override Prototype GetPrototype()
        {
            if (prototype == null)
                prototype = Prototype.Create(GetType());
            return prototype;
        }
    }
#endif
}
