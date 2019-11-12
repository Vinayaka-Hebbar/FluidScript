namespace FluidScript
{
    public abstract class RuntimeObject
    {
        protected static readonly object Any = new object();

        public static readonly RuntimeObject Null = new Core.PrimitiveObject(Any, PrimitiveType.Any);
        public static readonly RuntimeObject Zero = new Core.PrimitiveObject(0, PrimitiveType.Double);
        public static readonly RuntimeObject True = new Core.PrimitiveObject(true, PrimitiveType.Bool);
        public static readonly RuntimeObject False = new Core.PrimitiveObject(false, PrimitiveType.Bool);
        public static readonly RuntimeObject NaN = new Core.PrimitiveObject(double.NaN, PrimitiveType.Double);
        internal bool IsReturn;

        public virtual RuntimeObject Call(string name, params RuntimeObject[] args)
        {
            return Compiler.Reflection.MemberInvoker.Invoke(this, name, args);
        }

        public virtual RuntimeObject this[string name]
        {
            get
            {
                return Compiler.Reflection.MemberInvoker.Invoke(this, name);
            }
        }


        public abstract bool ToBool();

        public abstract char ToChar();

        public abstract int ToInt32();

        public abstract float ToFloat();

        public abstract double ToDouble();

        public abstract double ToNumber();

        public abstract bool IsNumber();

        public abstract bool IsString();

        public abstract bool IsBool();

        public abstract bool IsChar();

        public abstract bool IsArray();

        public virtual bool IsNull()
        {
            return true;
        }

        public virtual PrimitiveType RuntimeType
        {
            get => PrimitiveType.Any;
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public virtual System.Type DeclaredType
        {
            get => GetType();
        }

        public virtual object Instance()
        {
            return Any;
        }

        [Compiler.Reflection.Callable("toString")]
        internal RuntimeObject ToStringValue()
        {
            return new Core.PrimitiveObject(ToString());
        }

        #region Implicit
        public static implicit operator RuntimeObject(float value)
        {
            return new Core.PrimitiveObject(value);
        }

        public static implicit operator RuntimeObject(double value)
        {
            return new Core.PrimitiveObject(value);
        }

        public static implicit operator RuntimeObject(int value)
        {
            return new Core.PrimitiveObject(value);
        }

        public static implicit operator RuntimeObject(string value)
        {
            return new Core.PrimitiveObject(value);
        }

        public static implicit operator RuntimeObject(bool value)
        {
            return new Core.PrimitiveObject(value);
        }

        public static implicit operator RuntimeObject(char value)
        {
            return new Core.PrimitiveObject(value);
        }
        #endregion

        public static RuntimeObject operator +(RuntimeObject result1, RuntimeObject result2)
        {
            if (result1.IsNumber() && result2.IsNumber())
                return new Core.PrimitiveObject(result1.ToNumber() + result2.ToNumber());
            if (result1.IsString() || result2.IsString())
                return new Core.PrimitiveObject(result1.ToString() + result2.ToString());
            return Core.PrimitiveObject.Zero;
        }

        public static RuntimeObject operator -(RuntimeObject result1, RuntimeObject result2)
        {
            return new Core.PrimitiveObject(result1.ToNumber() - result2.ToNumber());
        }

        public static RuntimeObject operator *(RuntimeObject result1, RuntimeObject result2)
        {
            return new Core.PrimitiveObject(result1.ToNumber() * result2.ToNumber());
        }

        public static RuntimeObject operator /(RuntimeObject result1, RuntimeObject result2)
        {
            return new Core.PrimitiveObject(result1.ToNumber() / result2.ToNumber());
        }

        public static RuntimeObject operator %(RuntimeObject result1, RuntimeObject result2)
        {
            return new Core.PrimitiveObject(result1.ToNumber() % result2.ToNumber());
        }

        public static RuntimeObject operator &(RuntimeObject result1, RuntimeObject result2)
        {
            return new Core.PrimitiveObject((int)result1 & (int)result2);
        }

        public static RuntimeObject operator |(RuntimeObject result1, RuntimeObject result2)
        {
            return new Core.PrimitiveObject(result1.ToInt32() | result2.ToInt32());
        }

        public static RuntimeObject operator ^(RuntimeObject result1, RuntimeObject result2)
        {
            return new Core.PrimitiveObject(result1.ToInt32() ^ result2.ToInt32());
        }

        public static RuntimeObject operator ~(RuntimeObject result1)
        {
            result1++;
            return new Core.PrimitiveObject((int)result1);
        }

        public static RuntimeObject operator ++(RuntimeObject result1)
        {
            double value = result1.ToNumber();
            value++;
            return new Core.PrimitiveObject(value);
        }

        public static RuntimeObject operator --(RuntimeObject result1)
        {
            double value = result1.ToNumber();
            value--;
            return new Core.PrimitiveObject(value);
        }

        public static RuntimeObject operator >(RuntimeObject result1, RuntimeObject result2)
        {
            return new Core.PrimitiveObject(result1.ToNumber() > result2.ToNumber());
        }

        public static RuntimeObject operator >=(RuntimeObject result1, RuntimeObject result2)
        {
            return new Core.PrimitiveObject(result1.ToNumber() >= result2.ToNumber());
        }

        public static RuntimeObject operator <(RuntimeObject result1, RuntimeObject result2)
        {
            return new Core.PrimitiveObject(result1.ToNumber() < result2.ToNumber());
        }

        public static RuntimeObject operator <=(RuntimeObject result1, RuntimeObject result2)
        {
            return new Core.PrimitiveObject(result1.ToNumber() <= result2.ToNumber());
        }

        public static RuntimeObject operator ==(RuntimeObject result1, RuntimeObject result2)
        {
            return new Core.PrimitiveObject(result1.Equals(result2));
        }

        public static RuntimeObject operator !=(RuntimeObject result1, RuntimeObject result2)
        {
            return new Core.PrimitiveObject(!result1.Equals(result2));
        }

        public static RuntimeObject operator !(RuntimeObject result1)
        {
            if (result1.IsBool())
                return new Core.PrimitiveObject(!result1.ToBool());
            return Core.PrimitiveObject.Zero;
        }

        public static RuntimeObject operator +(RuntimeObject result1, int value)
        {
            if (result1.IsNumber())
                return new Core.PrimitiveObject(result1.ToNumber() + value);
            return new Core.PrimitiveObject(value);
        }

        public static RuntimeObject operator +(RuntimeObject result1)
        {
            if (result1.IsNumber())
                return new Core.PrimitiveObject(+result1.ToNumber());
            return Core.PrimitiveObject.Zero;
        }

        public static RuntimeObject operator -(RuntimeObject result1, int value)
        {
            return new Core.PrimitiveObject(result1.ToNumber() - value);
        }

        public static RuntimeObject operator -(RuntimeObject result1)
        {
            if (result1.IsNumber())
                return new Core.PrimitiveObject(-result1.ToNumber());
            return Core.PrimitiveObject.Zero;
        }

        public static RuntimeObject operator <<(RuntimeObject result1, int value)
        {
            return new Core.PrimitiveObject((int)result1 << value);
        }

        public static RuntimeObject operator >>(RuntimeObject result1, int value)
        {
            return new Core.PrimitiveObject(result1.ToInt32() >> value);
        }

        public static explicit operator int(RuntimeObject result)
        {
            return result.ToInt32();
        }

        public static explicit operator float(RuntimeObject result)
        {
            return result.ToFloat();
        }

        public static explicit operator double(RuntimeObject result)
        {
            return result.ToDouble();
        }

        public static explicit operator string(RuntimeObject result)
        {
            return result.ToString();
        }

        public static explicit operator char(RuntimeObject result)
        {
            return result.ToChar();
        }


        public static explicit operator bool(RuntimeObject result)
        {
            return result.ToBool();
        }
    }
}
