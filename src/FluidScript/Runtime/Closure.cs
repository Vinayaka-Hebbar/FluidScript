namespace FluidScript.Runtime
{
    /// <summary>
    /// Passing Runtime arguments of bounded functions for lamda
    /// </summary>
    public sealed class Closure
    {
        public object[] Values;

        public Closure(object[] values)
        {
            Values = values;
        }
    }
}
