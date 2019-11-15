namespace FluidScript
{
#if Runtime
    public abstract class RuntimeObject : System.Dynamic.IDynamicMetaObjectProvider
    {
        internal readonly static object _null = "null";
        internal readonly static object _undefined = "undefined";

        public static readonly RuntimeObject Null = new Core.PrimitiveObject(_null, RuntimeType.Any);
        public static readonly RuntimeObject Zero = new Core.PrimitiveObject(0, RuntimeType.Double);
        public static readonly RuntimeObject True = new Core.PrimitiveObject(true, RuntimeType.Bool);
        public static readonly RuntimeObject False = new Core.PrimitiveObject(false, RuntimeType.Bool);
        public static readonly RuntimeObject NaN = new Core.PrimitiveObject(double.NaN, RuntimeType.Double);
        public static readonly RuntimeObject Undefined = new Core.PrimitiveObject(_undefined, RuntimeType.Undefined);


        public virtual RuntimeObject Call(string name, params RuntimeObject[] args)
        {
            return Compiler.Reflection.MemberInvoker.Invoke(this, name, args);
        }

        public virtual RuntimeObject GetConstantValue(string name)
        {
            return Null;
        }

        public virtual RuntimeObject this[string name]
        {
            get
            {
                return Compiler.Reflection.MemberInvoker.Invoke(this, name);
            }
            set
            {
                throw new System.Exception("Not implemented");
            }
        }

        public virtual RuntimeObject DynamicInvoke(RuntimeObject[] args)
        {
            return Undefined;
        }

        public virtual bool ContainsKey(object key)
        {
            return false;
        }

        public virtual bool IsArray()
        {
            return (ReflectedType & RuntimeType.Array)== RuntimeType.Array;
        }

        public virtual bool IsBool()
        {
            return ReflectedType == RuntimeType.Bool;
        }

        public virtual bool IsChar()
        {
            return ReflectedType == RuntimeType.Char;
        }

        public virtual bool IsNumber()
        {
            return ReflectedType == RuntimeType.Double || ReflectedType == RuntimeType.Int32;
        }

        public virtual bool IsString()
        {
            return ReflectedType == RuntimeType.String;
        }

        public virtual bool ToBool()
        {
            return IsNull();
        }

        public virtual char ToChar()
        {
            return char.MinValue;
        }

        public virtual double ToDouble()
        {
            return double.NaN;
        }

        public virtual float ToFloat()
        {
            return float.NaN;
        }

        public virtual int ToInt32()
        {
            return 0;
        }

        public virtual double ToNumber()
        {
            return double.NaN;
        }

        public virtual bool IsNull()
        {
            return true;
        }

        public virtual RuntimeType ReflectedType
        {
            get => RuntimeType.Any;
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

        public virtual Compiler.Metadata.Prototype GetPrototype()
        {
            return Compiler.Metadata.Prototype.Create(GetType());
        }

        public virtual object Instance()
        {
            return _null;
        }

        public RuntimeObject Merge(RuntimeObject other)
        {
            var prototype = Compiler.Metadata.Prototype.Merge(GetPrototype(), other.GetPrototype());
            return prototype.CreateInstance();
        }

        [Compiler.Reflection.Callable("toString")]
        internal RuntimeObject ToStringValue()
        {
            return new Core.PrimitiveObject(ToString());
        }

        public System.Dynamic.DynamicMetaObject GetMetaObject(System.Linq.Expressions.Expression parameter)
        {
            return new Compiler.Metadata.MetaObject(parameter, this);
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

        public static RuntimeObject CreateReference(System.Func<RuntimeObject> action)
        {
            return CreateReference((System.Delegate)action);
        }

        public static RuntimeObject CreateReference(System.Func<RuntimeObject[], RuntimeObject> action)
        {
            return CreateReference((System.Delegate)action);
        }

        public static RuntimeObject CreateReference(System.Func<RuntimeObject, RuntimeObject> action)
        {
            return CreateReference((System.Delegate)action);
        }

        public static RuntimeObject CreateReference(System.Func<RuntimeObject, RuntimeObject, RuntimeObject> action)
        {
            return CreateReference((System.Delegate)action);
        }

        public static RuntimeObject CreateReference(System.Action action)
        {
            return CreateReference((System.Delegate)action);
        }

        public static RuntimeObject CreateReference(System.Action<RuntimeObject> action)
        {
            return CreateReference((System.Delegate)action);
        }

        public static RuntimeObject CreateReference(System.Action<RuntimeObject[]> action)
        {
            return CreateReference((System.Delegate)action);
        }

        public static RuntimeObject CreateReference(System.Action<RuntimeObject, RuntimeObject, RuntimeObject> action)
        {
            return CreateReference((System.Delegate)action);
        }

        public static RuntimeObject CreateReference(System.Delegate action)
        {
            var method = action.Method;
            var paramters = method.GetParameters();
            var types = new Compiler.Emit.ArgumentType[paramters.Length];
            var returnType = Compiler.Emit.TypeUtils.ToPrimitive(method.ReturnType);
            for (int index = 0; index < paramters.Length; index++)
            {
                System.Reflection.ParameterInfo arg = paramters[index];
                types[index] = new Compiler.Emit.ArgumentType(arg.Name, Compiler.Emit.TypeUtils.ToPrimitive(arg.ParameterType));
            }
            return new Compiler.Metadata.FunctionReference(action.Target, types, returnType, method);
        }
        #endregion

        public static RuntimeObject operator +(RuntimeObject result1, RuntimeObject result2)
        {
            if (result1.IsNumber() && result2.IsNumber())
                return new Core.PrimitiveObject(result1.ToNumber() + result2.ToNumber());
            if (result1.IsString() || result2.IsString())
                return new Core.PrimitiveObject(result1.ToString() + result2.ToString());
            return Zero;
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

        public static RuntimeObject From(object value)
        {
            if (value is RuntimeObject)
                return (RuntimeObject)value;
            if (value is null)
                return Null;
            if (value is System.IConvertible)
            {
                switch (value)
                {
                    case int _:
                        return System.Convert.ToInt32(value);
                    case double _:
                        return System.Convert.ToDouble(value);
                }
            }
            if(value is System.Array)
            {
                var array = (System.Array)value;
                RuntimeObject[] result = new RuntimeObject[array.Length];
                for(int i = 0;i< array.Length;i++)
                {
                    result[i] = From(array.GetValue(i));
                }
                return new Core.ArrayObject(result, RuntimeType.Any);
            }
            return value.ToString();
        }
    }
#endif
}
