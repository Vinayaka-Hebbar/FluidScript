namespace Scripting.Runtime
{
    public struct NamedObject
    {
        public readonly string Name;
        public readonly Object Value;

        public NamedObject(string name, Object value)
        {
            Name = name;
            Value = value;
        }

        public static implicit operator NamedObject(Object value)
        {
            return new NamedObject(string.Empty, value);
        }

        public static implicit operator Object(NamedObject namedObject)
        {
            return namedObject.Value;
        }
        public static implicit operator System.Collections.Generic.KeyValuePair<string, Object>(NamedObject namedObject)
        {
            return new System.Collections.Generic.KeyValuePair<string, Object>(namedObject.Name, namedObject.Value);
        }

    }
}
