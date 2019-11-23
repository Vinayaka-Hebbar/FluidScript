using FluidScript.Compiler.Metadata;
using System.Collections.Generic;

namespace FluidScript
{
    /// <summary>
    /// Represents the runtime instance
    /// </summary>
    public class RuntimeObject : System.Dynamic.IDynamicMetaObjectProvider, IEnumerable<object>
    {
        public static readonly RuntimeObject Null = new RuntimeObject(ObjectPrototype.Default);
        public static readonly RuntimeObject Undefined = new RuntimeObject(ObjectPrototype.Default, RuntimeType.Undefined);


        internal readonly Compiler.Reflection.Instances instances;

        private readonly Prototype prototype;

        public RuntimeObject(Prototype prototype)
        {
            this.prototype = prototype;
            instances = prototype.Init(this);
        }

        private RuntimeObject(Prototype prototype, RuntimeType type)
        {
            this.prototype = prototype;
            instances = prototype.Init(this);
            ReflectedType = type;
        }

        protected RuntimeObject(Prototype prototype, RuntimeObject obj)
        {
            this.prototype = prototype;
            instances = prototype.Init(this, new KeyValuePair<object, RuntimeObject>("this", obj));
        }

        protected RuntimeObject()
        {
            prototype = GetPrototype();
            if (prototype is null)
            {
                throw new System.NullReferenceException(nameof(prototype));
            }
            instances = prototype.Init(this);
        }

        public virtual RuntimeObject Call(string name, params RuntimeObject[] args)
        {
            if (instances.ContainsKey(name))
            {
                return instances[name].DynamicInvoke(args);
            }
            return Compiler.Reflection.TypeHelper.Invoke(this, name, args);
        }

        public virtual RuntimeObject this[string name]
        {
            get
            {
                if (instances.ContainsKey(name))
                    return instances[name];
                return Undefined;
            }
            set
            {
                if (value.ReflectedType == RuntimeType.Function)
                {
                    AttachFunction(name, value);
                    return;
                }
                instances[name] = value;
            }
        }

        public void Append(string name, RuntimeObject value, bool isReadOnly = false)
        {
            instances.Add(name, value, isReadOnly: isReadOnly);
        }

        private void AttachFunction(string name, RuntimeObject value)
        {
            FunctionGroup list = null;
            if (instances.TryGetValue(name, out RuntimeObject runtime))
            {
                if (runtime is FunctionGroup)
                {
                    list = (FunctionGroup)value;
                }
            }
            if (list is null)
            {
                list = new FunctionGroup(name);
                instances[name] = list;
            }
            list.Add((IFunctionReference)value);
        }

        public virtual bool ContainsKey(object key)
        {
            return instances != null ? instances.ContainsKey(key) : false;
        }

        public virtual RuntimeObject DynamicInvoke(RuntimeObject[] args)
        {
            return Undefined;
        }

        public virtual RuntimeType ReflectedType { get; } = RuntimeType.Any;

        public override string ToString()
        {
            return instances.ToString();
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

        #region Convert
        public virtual bool IsArray()
        {
            return (ReflectedType & RuntimeType.Array) == RuntimeType.Array;
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
        #endregion

        public virtual Prototype GetPrototype()
        {
            if (prototype is null)
                return Prototype.Create(GetType());
            return prototype;
        }

        public RuntimeObject Merge(RuntimeObject other)
        {
            var prototype = Prototype.Merge(GetPrototype(), other.GetPrototype());
            return prototype.CreateInstance();
        }

        [Compiler.Reflection.Callable("toString", RuntimeType.String)]
        internal RuntimeObject ToStringValue()
        {
            return new Library.StringObject(ToString());
        }

        System.Dynamic.DynamicMetaObject System.Dynamic.IDynamicMetaObjectProvider.GetMetaObject(System.Linq.Expressions.Expression parameter)
        {
            return new Compiler.Metadata.MetaObject(parameter, this);
        }

        #region Implicit
        public static implicit operator RuntimeObject(float value)
        {
            return new Library.PrimitiveObject(value);
        }

        public static implicit operator RuntimeObject(double value)
        {
            return new Library.PrimitiveObject(value);
        }

        public static implicit operator RuntimeObject(int value)
        {
            return new Library.PrimitiveObject(value);
        }

        public static implicit operator RuntimeObject(string value)
        {
            return new Library.StringObject(value);
        }

        public static implicit operator RuntimeObject(bool value)
        {
            return new Library.PrimitiveObject(value);
        }

        public static implicit operator RuntimeObject(char value)
        {
            return new Library.PrimitiveObject(value);
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
            return new FunctionReference(action.Target, types, returnType, method);
        }
        #endregion

        public static RuntimeObject operator +(RuntimeObject result1, RuntimeObject result2)
        {
            if (result1.IsNumber() && result2.IsNumber())
                return new Library.PrimitiveObject(result1.ToNumber() + result2.ToNumber());
            if (result1.IsString() || result2.IsString())
                return new Library.StringObject(result1.ToString() + result2.ToString());
            return Library.PrimitiveObject.Zero;
        }

        public static RuntimeObject operator -(RuntimeObject result1, RuntimeObject result2)
        {
            return new Library.PrimitiveObject(result1.ToNumber() - result2.ToNumber());
        }

        public static RuntimeObject operator *(RuntimeObject result1, RuntimeObject result2)
        {
            return new Library.PrimitiveObject(result1.ToNumber() * result2.ToNumber());
        }

        public static RuntimeObject operator /(RuntimeObject result1, RuntimeObject result2)
        {
            return new Library.PrimitiveObject(result1.ToNumber() / result2.ToNumber());
        }

        public static RuntimeObject operator %(RuntimeObject result1, RuntimeObject result2)
        {
            return new Library.PrimitiveObject(result1.ToNumber() % result2.ToNumber());
        }

        public static RuntimeObject operator &(RuntimeObject result1, RuntimeObject result2)
        {
            return new Library.PrimitiveObject((int)result1 & (int)result2);
        }

        public static RuntimeObject operator |(RuntimeObject result1, RuntimeObject result2)
        {
            return new Library.PrimitiveObject(result1.ToInt32() | result2.ToInt32());
        }

        public static RuntimeObject operator ^(RuntimeObject result1, RuntimeObject result2)
        {
            return new Library.PrimitiveObject(result1.ToInt32() ^ result2.ToInt32());
        }

        public static RuntimeObject operator ~(RuntimeObject result1)
        {
            result1++;
            return new Library.PrimitiveObject((int)result1);
        }

        public static RuntimeObject operator ++(RuntimeObject result1)
        {
            double value = result1.ToNumber();
            value++;
            return new Library.PrimitiveObject(value);
        }

        public static RuntimeObject operator --(RuntimeObject result1)
        {
            double value = result1.ToNumber();
            value--;
            return new Library.PrimitiveObject(value);
        }

        public static RuntimeObject operator >(RuntimeObject result1, RuntimeObject result2)
        {
            return new Library.PrimitiveObject(result1.ToNumber() > result2.ToNumber());
        }

        public static RuntimeObject operator >=(RuntimeObject result1, RuntimeObject result2)
        {
            return new Library.PrimitiveObject(result1.ToNumber() >= result2.ToNumber());
        }

        public static RuntimeObject operator <(RuntimeObject result1, RuntimeObject result2)
        {
            return new Library.PrimitiveObject(result1.ToNumber() < result2.ToNumber());
        }

        public static RuntimeObject operator <=(RuntimeObject result1, RuntimeObject result2)
        {
            return new Library.PrimitiveObject(result1.ToNumber() <= result2.ToNumber());
        }

        public static RuntimeObject operator ==(RuntimeObject result1, RuntimeObject result2)
        {
            return new Library.PrimitiveObject(result1.Equals(result2));
        }

        public static RuntimeObject operator !=(RuntimeObject result1, RuntimeObject result2)
        {
            return new Library.PrimitiveObject(!result1.Equals(result2));
        }

        public static RuntimeObject operator !(RuntimeObject result1)
        {
            if (result1.IsBool())
                return new Library.PrimitiveObject(!result1.ToBool());
            return Library.PrimitiveObject.Zero;
        }

        public static RuntimeObject operator +(RuntimeObject result1, int value)
        {
            if (result1.IsNumber())
                return new Library.PrimitiveObject(result1.ToNumber() + value);
            return new Library.PrimitiveObject(value);
        }

        public static RuntimeObject operator +(RuntimeObject result1)
        {
            if (result1.IsNumber())
                return new Library.PrimitiveObject(+result1.ToNumber());
            return Library.PrimitiveObject.Zero;
        }

        public static RuntimeObject operator -(RuntimeObject result1, int value)
        {
            return new Library.PrimitiveObject(result1.ToNumber() - value);
        }

        public static RuntimeObject operator -(RuntimeObject result1)
        {
            if (result1.IsNumber())
                return new Library.PrimitiveObject(-result1.ToNumber());
            return Library.PrimitiveObject.Zero;
        }

        public static RuntimeObject operator <<(RuntimeObject result1, int value)
        {
            return new Library.PrimitiveObject((int)result1 << value);
        }

        public static RuntimeObject operator >>(RuntimeObject result1, int value)
        {
            return new Library.PrimitiveObject(result1.ToInt32() >> value);
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
                var type = value.GetType();
                if (type.IsPrimitive)
                {
                    return new Library.PrimitiveObject(value, type);
                }
                //todo Iconvert to runtime
            }
            if (value is System.Collections.IList array)
            {
                RuntimeObject[] result = new RuntimeObject[array.Count];
                for (int i = 0; i < array.Count; i++)
                {
                    result[i] = From(array[i]);
                }
                return new Library.ArrayObject(result, RuntimeType.Any);
            }
            return value.ToString();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return instances.GetEnumerator();
        }

        IEnumerator<object> IEnumerable<object>.GetEnumerator()
        {
            return instances.GetEnumerator();
        }
    }
}
