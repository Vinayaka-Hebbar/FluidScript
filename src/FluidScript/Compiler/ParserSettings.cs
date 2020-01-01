using System;
using System.Globalization;

namespace FluidScript.Compiler
{
    public class ParserSettings
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
            NumberStyle = NumberStyles.Float;
            FormatProvider = CultureInfo.InvariantCulture;
        }

        public NumberStyles NumberStyle { get; set; }

        public IFormatProvider FormatProvider { get; set; }
    }
}
