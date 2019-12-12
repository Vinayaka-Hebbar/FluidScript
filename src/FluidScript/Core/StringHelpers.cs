using System;
using System.Collections.Generic;
using System.Text;

namespace FluidScript.Library
{
    internal static class StringHelpers
    {
        internal static string Join<T>(string separator, IEnumerable<T> values)
        {
            if (separator == null)
                throw new ArgumentNullException(nameof(separator));
            if (values == null)
                throw new ArgumentNullException(nameof(values));
            var result = new StringBuilder();
            bool first = true;
            foreach (object value in values)
            {
                if (first == false)
                    result.Append(separator);
                first = false;
                result.Append(value);
            }
            return result.ToString();
        }
    }
}
