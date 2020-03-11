using FluidScript.Runtime;

namespace FluidScript
{
    public class GlobalObject
    {
        protected GlobalObject()
        {
        }

        #region Static Objects

        [Register("Object")]
        public static FSObject Object { get; }

        [Register("Math")]
        public static Math Math { get; }

        /// <summary>
        /// Global target for compilation
        /// </summary>
        [Register("lib")]
        public static GlobalObject Instance { get; } = new GlobalObject();

        DynamicObject m_global;

        [Register("global")]
        public virtual DynamicObject Global
        {
            get
            {
                if (m_global == null)
                    m_global = new DynamicObject();
                return m_global;
            }
        }
        #endregion

        [Register("print")]
        public virtual void Print(String format, params object[] args)
        {
            System.Console.WriteLine(format.ToString(), args);
        }

        [Register("print")]
        public virtual void Print(object value)
        {
            System.Console.WriteLine(value);
        }

        [Register("len")]
        public static Integer Length(System.Collections.IEnumerable value)
        {
            if (value is null)
                return 0;
            if (value is System.Collections.ICollection collection)
                return collection.Count;
            int count = 0;
            System.Collections.IEnumerator e = value.GetEnumerator();
            checked
            {
                while (e.MoveNext()) count++;
            }
            return count;
        }

        [Register("parseInt")]
        public static Integer ParseInt(String s)
        {
            if (!(s is null) && int.TryParse(s.ToString(), out int value))
                return new Integer(value);
            return new Integer(0);
        }

        [Register("parseNumber")]
        public static Double ParseNumber(String s)
        {
            if (!(s is null) && double.TryParse(s.ToString(), out double value))
                return new Double(value);
            return new Double(0);
        }

        [Register("toString")]
        public override string ToString()
        {
            return base.ToString();
        }

        [Register("isEquals")]
        public static bool IsEquals(object objA, object objB)
        {
            return objA == objB;
        }
    }
}
