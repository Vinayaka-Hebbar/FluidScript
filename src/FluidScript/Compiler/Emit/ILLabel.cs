using System.Reflection.Emit;

namespace FluidScript.Compiler.Emit
{
    /// <summary>
    /// Represents a label in IL code.
    /// </summary>
    public sealed class ILLabel
    {
        public readonly Label UnderlyingLabel;

        public ILLabel(Label label)
        {
            if (label == null)
                throw new System.ArgumentNullException(nameof(label));
            UnderlyingLabel = label;
        }
    }
}
