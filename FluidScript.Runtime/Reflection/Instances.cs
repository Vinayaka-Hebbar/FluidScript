﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FluidScript.Reflection
{
    internal sealed class Instances : IEnumerable<object>
    {
        private readonly IDictionary<object, InstanceName> names;

        private readonly IList<RuntimeObject> values;

        internal Instances()
        {
            names = new Dictionary<object, InstanceName>();
            values = new List<RuntimeObject>();
        }

        internal RuntimeObject this[object key]
        {
            get
            {
                var name = names[key];
                return values[name.Index];
            }
            set
            {
                if (names.ContainsKey(key))
                {
                    var name = names[key];
                    if (name.IsReadOnly)
                        throw new System.Exception(string.Concat("Can't modify readonly variable ", key));
                    values[name.Index] = value;
                }
                else
                {
                    names[key] = new InstanceName(key, values.Count, false);
                    values.Add(value);
                }
            }
        }

        internal void AttachFunction(RuntimeObject obj, DeclaredMethod method)
        {
            if (method.Store != null)
            {
                Core.FunctionGroup list = null;
                if (TryGetValue(method.Name, out RuntimeObject existing))
                    if (existing is Core.FunctionGroup)
                        list = (Core.FunctionGroup)existing;
                if (list is null)
                {
                    list = new Core.FunctionGroup(method.Name);
                    Add(method.Name, list);
                }
                Core.IFunctionReference reference = method.Default;
                if (reference is null)
                    reference = new Core.FunctionReference(obj, method.Arguments, method.ReturnType, method.Store);
                list.Add(reference);
            }
        }

        internal void Add(object name, RuntimeObject value, bool isReadOnly = false)
        {
            names[name] = new InstanceName(name, values.Count, isReadOnly);
            values.Add(value);
        }

        internal bool TryGetValue(object key, out RuntimeObject value)
        {
            if (names.ContainsKey(key))
            {
                var name = names[key];
                value = values[name.Index];
                return true;
            }
            value = null;
            return false;
        }

        internal bool ContainsKey(object key)
        {
            return names.ContainsKey(key);
        }

        internal string ToStringLocal()
        {
            return string.Concat("\n{", string.Join(",", names.Skip(1).Select(item => string.Concat(item.Key, ":", values[item.Value.Index].ToString()))), "}");
        }

        public override string ToString()
        {
            return string.Concat("\n{", string.Join(",", names.Select(item => string.Concat(item.Key, ":", values[item.Value.Index].ToString()))), "}");
        }

        public IEnumerator<object> GetEnumerator()
        {
            return names.Keys.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return names.Keys.GetEnumerator();
        }
    }

    internal struct InstanceName
    {
        internal readonly object Key;
        public readonly int Index;
        internal readonly bool IsReadOnly;

        public InstanceName(object key, int index, bool isReadOnly)
        {
            Key = key;
            Index = index;
            IsReadOnly = isReadOnly;
        }

        public override string ToString()
        {
            return Key.ToString();
        }
    }
}