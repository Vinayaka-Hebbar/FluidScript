using FluidScript.Runtime;

namespace FluidScript
{
    /// <summary>
    /// Global object for runtime
    /// </summary>
    [Register("Lib")]
    public class GlobalObject : FSObject
    {
        private static readonly GlobalObject instance = new GlobalObject();

        protected GlobalObject()
        {
        }

        #region Static Objects

        [Register(nameof(Object), RegisterImplOption.Activator)]
        public static FSObject Object { get; }

        [Register(nameof(Math), RegisterImplOption.Activator)]
        public static Math Math { get; }

        [Register(nameof(Date), RegisterImplOption.Activator)]
        public static Date Date { get; }

        /// <summary>
        /// Global target for compilation
        /// </summary>
        [Register("lib", RegisterImplOption.Library)]
        public static GlobalObject Instance
        {
            get => instance;
        }
        #endregion

        [Register("print")]
        public virtual void Print(String format, params object[] args)
        {
            System.Console.WriteLine(format.ToString(), args);
        }

        /// <summary>
        /// Print specified object <paramref name="value"/>
        /// </summary>
        /// <param name="value"></param>
        [Register("print")]
        public virtual void Print(object value)
        {
            System.Console.WriteLine(value);
        }

        /// <summary>
        /// Get the length of <paramref name="value"/>
        /// </summary>
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

        /// <summary>
        ///  Generates a sequence of integral numbers within a specified range.
        /// </summary>
        /// <param name="start">The value of the first integer in the sequence.</param>
        /// <param name="count"> The number of sequential integers to generate.</param>
        /// <returns>An <see cref="System.Collections.Generic.IEnumerable{T}"/> in C# or IEnumerable(Of Int32) in Visual Basic that contains
        ///  a range of sequential integral numbers.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">count is less than 0.-or-start + count -1 is larger than System.Int32.MaxValue.</exception>
        [Register("range")]
        public static System.Collections.Generic.IEnumerable<Integer> Range(Integer start, Integer count)
        {
            return Range(start, count, 1);
        }

        /// <summary>
        ///  Generates a sequence of integral numbers within a specified range.
        /// </summary>
        /// <param name="start">The value of the first integer in the sequence.</param>
        /// <param name="count"> The number of sequential integers to generate.</param>
        /// <param name="increment">The value of increment </param>
        /// <returns>An <see cref="System.Collections.Generic.IEnumerable{T}"/> in C# or IEnumerable (Of Int32) in Visual Basic that contains
        ///  a range of sequential integral numbers.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">count is less than 0.-or-start + count -1 is larger than System.Int32.MaxValue.</exception>
        [Register("range")]
        public static System.Collections.Generic.IEnumerable<Integer> Range(Integer start, Integer count, Integer increment)
        {
            int inc = increment;
            int len = count;
            int s = start;
            long max = ((long)s) + len - 1;
            if (count < 0 || max > int.MaxValue) throw new System.ArgumentOutOfRangeException("count");
            for (int i = 0; i < len; i += inc) yield return new Integer(s + i);
        }

        /// <summary>
        /// Converts String to <see cref="Integer"/>
        /// </summary>
        [Register("parseInt")]
        public static Integer ParseInt(String s)
        {
            if (!(s is null) && int.TryParse(s.ToString(), out int value))
                return new Integer(value);
            return new Integer(0);
        }

        /// <summary>
        /// Converts String to <see cref="Double"/>
        /// </summary>
        [Register("parseNumber")]
        public static Double ParseNumber(String s)
        {
            if (!(s is null) && double.TryParse(s.ToString(), out double value))
                return new Double(value);
            return new Double(0);
        }
    }
}
