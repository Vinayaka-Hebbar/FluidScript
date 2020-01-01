namespace FluidScript
{
    /// <inheritdoc/>
    public class FSObject : IFSObject
    {
        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj is IFSObject)
                return __Equals((IFSObject)obj).m_value;
            return base.Equals(obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return ((IFSObject)this).HashCode().m_value;
        }

        /// <inheritdoc/>
        [Runtime.Register("hashCode")]
        public virtual Integer HashCode()
        {
            return base.GetHashCode();
        }

        /// <inheritdoc/>
        [Runtime.Register("toString")]
        public virtual String __ToString()
        {
            return base.ToString();
        }

        /// <inheritdoc/>
        [Runtime.Register("equals")]
        public virtual Boolean __Equals(IFSObject obj)
        {
            return ReferenceEquals(this, obj);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return __ToString().m_value;
        }

        /// <summary>
        /// Equal Implementation
        /// </summary>
        public static Boolean operator ==(FSObject left, FSObject right)
        {
            return left.__Equals(right);
        }

        /// <summary>
        /// Not Equal Implementation
        /// </summary>
        public static Boolean operator !=(FSObject left, FSObject right)
        {
            return left.__Equals(right).m_value ? Boolean.False : Boolean.True;
        }

        /// <summary>
        /// convert cs objects to fluid object
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [Runtime.Register("convert")]
        public static object Convert(object obj)
        {
            switch (obj)
            {
                case int i:
                    return new Integer(i);
                case sbyte b:
                    return new Byte(b);
                case short s:
                    return new Short(s);
                case long l:
                    return new Long(l);
                case float f:
                    return new Float(f);
                case double d:
                    return new Double(d);
                case bool b:
                    return new Boolean(b);
                case char c:
                    return new Char(c);
                case string s:
                    return new String(s);
                default:
                    return obj;

            }
        }
    }
}
