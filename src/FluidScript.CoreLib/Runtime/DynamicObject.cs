using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Runtime.Serialization;

namespace FluidScript.Runtime
{
    public interface IDynamicObject : IDynamicInvocable, IRuntimeMetadata
    {
        object GetValue(MemberKey key);
        void SetValue(MemberKey key, object value);
        object GetValue(string name);
        void SetValue(string name, object value);
        bool TryGetMember(string key, out MemberKey member);
        MemberKey Add(string name, System.Type type, object value);
    }

    // todo move this to coreLib
    /// <summary>
    /// Dynamic Runtime Object
    /// </summary>
    [Register(nameof(DynamicObject))]
    [System.Serializable]
    public class DynamicObject : Collections.DictionaryBase<MemberKey, object>,
        IDictionary<string, object>,
        ISerializable,
        IDynamicMetaObjectProvider,
        IDynamicObject
    {
        #region Static
        static readonly IEqualityComparer<MemberKey> DefaultComparer = EqualityComparer<MemberKey>.Default; 
        #endregion

        #region Constructors
        public DynamicObject(int capacity) : base(capacity, DefaultComparer)
        {

        }

        public DynamicObject() : base(3, DefaultComparer)
        {
        }

        public DynamicObject(IDictionary<string, object> values) : base(values.Count, DefaultComparer)
        {
            if (values == null)
            {
                throw new System.ArgumentNullException(nameof(values));
            }
            foreach (var item in values)
            {
                Add(item.Key, item.Value);
            }
        } 
        #endregion

        #region List And Dictionary
        public ICollection<string> Keys
        {
            get
            {
                var size = Count;
                string[] keys = new string[size];
                for (int index = 0; index < size; index++)
                {
                    keys[index] = entries[index].Key.Name;
                }
                return keys;
            }
        }

        ICollection<object> IDictionary<string, object>.Values
        {
            get
            {
                var size = Count;
                object[] values = new object[size];
                for (int index = 0; index < size; index++)
                {
                    values[index] = entries[index].Value;
                }
                return values;
            }
        }

        /// <summary>
        /// Getter and Setter of variables
        /// </summary>
        public object this[String name]
        {
            get
            {
                return GetValue(name);
            }
            set
            {
                SetValue(name, value);
            }
        }

        public void SetValue(string name, object value)
        {
            var i = FindEntry(name);
            if (i < 0)
            {
                Add(name, value);
            }
            else
            {
                entries[i].Value = value;
            }
        }

        public object GetValue(string name)
        {
            var i = FindEntry(name);
            if (i < 0)
            {
                return null;
            }
            return entries[i].Value;
        }

        int FindEntry(string key)
        {
            if (key == null)
            {
                throw new System.NullReferenceException(nameof(key));
            }
            if (buckets != null)
            {
                Entry[] items = entries;
                int hashCode = key.GetHashCode() & 0x7FFFFFFF;
                for (int i = buckets[hashCode % buckets.Length]; i >= 0; i = items[i].Next)
                {
                    if (items[i].HashCode == hashCode && string.Equals(items[i].Key.Name, key)) return i;
                }
            }
            return -1;
        }

        [Register("contains")]
        public bool ContainsKey(string key)
        {
            return FindEntry(key) >= 0;
        }

        /// <summary>
        /// Replaces specific value if key exist
        /// </summary>
        public void Add(string key, object value)
        {
            Add(key, value == null ? typeof(object) : value.GetType(), value);
        }

        public MemberKey Add(string key, System.Type type, object value)
        {
            if (key == null)
                throw new System.ArgumentNullException(nameof(key));
            return Insert(key, value, type);
        }

        internal MemberKey Insert(string name, object value, System.Type type)
        {
            if (buckets == null) Initialize(0);
            int hashCode = name.GetHashCode() & 0x7FFFFFFF;
            int targetBucket = hashCode % buckets.Length;
            for (int i = buckets[targetBucket]; i >= 0; i = entries[i].Next)
            {
                if (entries[i].HashCode == hashCode && string.Equals(entries[i].Key.Name, name))
                {
                    throw new System.ArgumentException(string.Format("shadow variable name '{0}'", name));
                }
            }
            int index;
            if (freeCount > 0)
            {
                index = freeList;
                freeList = entries[index].Next;
                freeCount--;
            }
            else
            {
                if (count == entries.Length)
                {
                    Resize(Helpers.ExpandPrime(count), false);
                    targetBucket = hashCode % buckets.Length;
                }
                index = count;
                count++;
            }
            var variable = new MemberKey(name, type, index, hashCode);
            entries[index].HashCode = hashCode;
            entries[index].Next = buckets[targetBucket];
            entries[index].Key = variable;
            entries[index].Value = value;
            buckets[targetBucket] = index;
            return variable;
        }

        #endregion

        #region Runtime Support

        [Register("keys")]
        public Collections.List<String> GetMemberNames()
        {
            var size = Count;
            var keys = new Collections.List<String>(size);
            for (int index = 0; index < size; index++)
            {
                var local = entries[index].Key;
                keys.Add(local.Name);
            }
            return keys;
        }

        [Register("clone")]
        public DynamicObject Clone()
        {
            return new DynamicObject(this);
        }

        [Register("toString")]
        public override string ToString()
        {
            return string.Join(",\n", Keys);
        }
        #endregion

        #region DictionaryEnumerator

        [System.Serializable]
        public struct Enumerator : IEnumerator<KeyValuePair<string, object>>, IDictionaryEnumerator
        {
            private readonly DynamicObject obj;
            private readonly int version;
            private int index;
            private KeyValuePair<string, object> current;
            private readonly int getEnumeratorRetType;  // What should Enumerator.Current return?

            internal const int DictEntry = 1;
            internal const int KeyValuePair = 2;

            internal Enumerator(DynamicObject value, int getEnumeratorRetType)
            {
                obj = value;
                version = value.version;
                index = 0;
                this.getEnumeratorRetType = getEnumeratorRetType;
                current = new KeyValuePair<string, object>();
            }

            public bool MoveNext()
            {
                if (version != obj.version)
                {
                    throw new System.InvalidOperationException(nameof(version));
                }

                // Use unsigned comparison since we set index to dictionary.count+1 when the enumeration ends.
                // dictionary.count+1 could be negative if dictionary.count is Int32.MaxValue
                while ((uint)index < (uint)obj.count)
                {
                    if (obj.entries[index].HashCode >= 0)
                    {
                        current = new KeyValuePair<string, object>(obj.entries[index].Key.Name, obj.entries[index].Value);
                        index++;
                        return true;
                    }
                    index++;
                }

                index = obj.count + 1;
                current = new KeyValuePair<string, object>();
                return false;
            }

            public KeyValuePair<string, object> Current
            {
                get { return current; }
            }

            public void Dispose()
            {
            }

            object IEnumerator.Current
            {
                get
                {
                    if (index == 0 || (index == obj.count + 1))
                    {
                        throw new System.InvalidOperationException("Operation can't happen");
                    }

                    if (getEnumeratorRetType == DictEntry)
                    {
                        return new DictionaryEntry(current.Key, current.Value);
                    }
                    else
                    {
                        return new KeyValuePair<string, object>(current.Key, current.Value);
                    }
                }
            }

            void IEnumerator.Reset()
            {
                if (version != obj.version)
                {
                    throw new System.InvalidOperationException(nameof(version));
                }

                index = 0;
                current = new KeyValuePair<string, object>();
            }

            DictionaryEntry IDictionaryEnumerator.Entry
            {
                get
                {
                    if (index == 0 || (index == obj.count + 1))
                    {
                        throw new System.InvalidOperationException("Operation can't happen");
                    }

                    return new DictionaryEntry(current.Key, current.Value);
                }
            }

            object IDictionaryEnumerator.Key
            {
                get
                {
                    if (index == 0 || (index == obj.count + 1))
                    {
                        throw new System.InvalidOperationException("Operation can't happen");
                    }

                    return current.Key;
                }
            }

            object IDictionaryEnumerator.Value
            {
                get
                {
                    if (index == 0 || (index == obj.count + 1))
                    {
                        throw new System.InvalidOperationException("Operation can't happen");
                    }

                    return current.Value;
                }
            }
        }
        #endregion

        #region Serializable
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            var size = Count;
            //todo list serialize
            for (int index = 0; index < size; index++)
            {
                var local = entries[index];
                if (local.HashCode >= 0)
                {
                    var key = local.Key;
                    var value = local.Value;
                    if (key.Type.IsPrimitive)
                        info.AddValue(key.Name, value, key.Type);
                    else if (value is System.IConvertible convertible)
                    {
                        info.AddValue(key.Name, System.Convert.ChangeType(value, convertible.GetTypeCode()), key.Type);
                    }
                    else if (value is String)
                    {
                        info.AddValue(key.Name, value.ToString(), typeof(string));
                    }
                    else
                    {
                        info.AddValue(key.Name, value);
                    }
                }
            }

        }
        #endregion

        #region MetaObjectProvider
        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(System.Linq.Expressions.Expression parameter)
        {
            return new MetaObject(parameter, this);
        }

        bool IRuntimeMetadata.GetOrCreateBinder(string name, object value, System.Type type, out IMemberBinder binder)
        {
            var i = FindEntry(name);
            if (i >= 0)
            {
                binder = new DynamicBinder(entries[i].Key);
                return true;
            }
            binder = new DynamicBinder(Insert(name, value, type));
            return true;
        }

        bool IRuntimeMetadata.TryGetBinder(string name, out IMemberBinder binder)
        {
            var i = FindEntry(name);
            if (i >= 0)
            {
                binder = new DynamicBinder(entries[i].Key);
                return true;
            }
            binder = null;
            return false;
        }
        #endregion

        #region IDictionary

        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
        {
            return new Enumerator(this, Enumerator.KeyValuePair);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this, Enumerator.KeyValuePair);
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this, Enumerator.KeyValuePair);
        }

        object IDictionary<string, object>.this[string key]
        {
            get
            {
                return GetValue(key);
            }
            set
            {
                SetValue(key, value);
            }
        }

        bool IDictionary<string, object>.TryGetValue(string key, out object value)
        {
            int i = FindEntry(key);
            if (i >= 0)
            {
                value = entries[i].Value;
                return true;
            }
            value = null;
            return false;
        }

        void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item)
        {
            Add(item.Key, item.Value);
        }

        void ICollection<KeyValuePair<string, object>>.Clear()
        {
            Clear();
        }

        bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item)
        {
            int i = FindEntry(item.Key);
            if (i >= 0)
            {
                var value = entries[i].Value;
                if (EqualityComparer<object>.Default.Equals(value, item.Value))
                    return true;
            }
            return false;
        }

        void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int index)
        {
            if (array == null)
            {
                throw new System.ArgumentNullException(nameof(array));
            }

            if (index < 0 || index > array.Length)
            {
                throw new System.ArgumentOutOfRangeException(string.Concat("Index ", index, " out of range"));
            }

            if (array.Length - index < Count)
            {
                throw new System.ArgumentException("array too small");
            }

            int count = Count;
            Entry[] entries = this.entries;
            for (int i = 0; i < count; i++)
            {
                var entry = entries[i];
                array[index++] = new KeyValuePair<string, object>(entry.Key.Name, entry.Value);
            }
        }

        public bool Remove(string key)
        {
            var i = FindEntry(key);
            if (i >= 0)
            {
                return Remove(entries[i].Key);
            }
            return false;
        }

        bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
        {
            int i = FindEntry(item.Key);
            if (i >= 0)
            {
                var entry = entries[i];
                if (EqualityComparer<object>.Default.Equals(entry.Value, item.Value))
                    return Remove(entry.Key);
            }
            return false;
        }
        #endregion

        #region IRuntimeMetadata

        object IDynamicObject.GetValue(MemberKey key)
        {
            var i = FindEntry(key);
            if (i < 0)
            {
                return null;
            }
            return entries[i].Value;
        }

        void IDynamicObject.SetValue(MemberKey key, object value)
        {
            var i = FindEntry(key);
            if (i < 0)
            {
                throw new KeyNotFoundException(key.Name);
            }
            entries[i].Value = value;
            return;
        }

        /// <summary>
        /// Get current context variable
        /// </summary>
        bool IDynamicObject.TryGetMember(string key, out MemberKey member)
        {
            int i = FindEntry(key);
            if (i >= 0)
            {
                member = entries[i].Key;
                return true;
            }
            member = default(MemberKey);
            return false;
        }

        Any IDynamicInvocable.SafeSetValue(Any value, string name, System.Type type)
        {
            var i = FindEntry(name);
            if (i < 0)
            {
                Insert(name, value.m_value, type);
            }
            else
            {
                var member = entries[i].Key;
                System.Type dest = member.Type;
                if (value.m_value == null)
                {
                    if (dest.IsNullAssignable())
                        entries[i].Value = value.m_value;
                    else
                        throw new System.Exception(string.Concat("Can't assign null value to type ", dest));
                }
                else if (TypeUtils.AreReferenceAssignable(dest, type))
                {
                    entries[i].Value = value.m_value;
                }
                else if (type.TryImplicitConvert(dest, out System.Reflection.MethodInfo implConvert))
                {
                    value = Any.op_Implicit(implConvert.Invoke(null, new object[1] { value.m_value }));
                    entries[i].Value = value.m_value;
                }
                else
                {
                    throw new System.InvalidCastException(string.Concat(type, " to ", dest));
                }
            }
            return value;
        }

        Any IDynamicInvocable.Invoke(string name, params Any[] args)
        {
            var actualArgs = Any.GetArgs(args);
            var i = FindEntry(name);
            if (i >= 0 && entries[i].Value is System.Delegate del)
            {
                var conversions = new ArgumentConversions(args.Length);
                var method = del.GetType().GetMethod(nameof(System.Action.Invoke));
                if (method.MatchesArguments(actualArgs, conversions))
                {
                    return Any.op_Implicit(del.DynamicInvoke(actualArgs));
                }
            }
            return default(Any);
        }

        Any IDynamicInvocable.SafeGetValue(string name)
        {
            var i = FindEntry(name);
            if (i >= 0)
            {
                return Any.op_Implicit(entries[i].Value);
            }
            return default(Any);
        }
        #endregion
    }
}