using System;
using System.Collections.Generic;
using System.Globalization;

namespace FluidScript.Compiler
{
    public sealed class ParserSettings
    {
        private static ParserSettings _default;
        public static ParserSettings Default
        {
            get
            {
                if (_default == null)
                    _default = new ParserSettings();
                return _default;
            }
        }
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
