using Microsoft.VisualBasic.CompilerServices;
using System.Runtime.CompilerServices;

namespace FluidScript.Primitives
{
    public struct Any : IStrongBox
    {
        object value;

        object IStrongBox.Value { get => value; set => this.value = value; }

        public Any(object value)
        {
            this.value = value;
        }

        public static implicit operator Any(Double i) => new Any(i);
        public static implicit operator Any(Float i) => new Any(i);
        public static implicit operator Any(Integer i) => new Any(i);
        public static implicit operator Any(Char i) => new Any(i);
        public static implicit operator Any(String i) => new Any(i);
        }
    }
}
