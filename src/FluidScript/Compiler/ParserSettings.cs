using System;
using System.Collections.Generic;
using System.Globalization;

namespace FluidScript.Compiler
{
    public sealed class ParserSettings
    {
        public ParserSettings()
        {
            Comparer = EqualityComparer<string>.Default;
            NumberStyle = NumberStyles.Float;
            FormatProvider = CultureInfo.InvariantCulture;
        }

        public IEqualityComparer<string> Comparer { get; set; }
        public NumberStyles NumberStyle { get; set; }
        public IFormatProvider FormatProvider { get; set; }
    }
}
