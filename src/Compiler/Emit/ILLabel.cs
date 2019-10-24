using System.Reflection.Emit;

namespace FluidScript.Compiler.Emit
{
    /// <summary>
    /// Represents a label in IL code.
    /// </summary>
    public abstract class ILLabel
    {
    }

    public sealed class ReflectionILLabel : ILLabel
    {
        public readonly Label UnderlyingLabel;

        public ReflectionILLabel(Label label)
        {
            if (label == null)
                throw new System.ArgumentNullException(nameof(label));
            UnderlyingLabel = label;
        }
    }
}
