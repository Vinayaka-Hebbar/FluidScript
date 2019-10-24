using System.Reflection.Emit;

namespace FluidScript.Compiler.Reflection
{
    public interface IReflection
    {
        void Generate(TypeBuilder builder);
    }
}