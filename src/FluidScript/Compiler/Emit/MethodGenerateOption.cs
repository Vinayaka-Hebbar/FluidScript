namespace FluidScript.Compiler.Emit
{
    public enum MethodGenerateOption
    {
        None = 0,
        /// <summary>
        /// Need to duplicate the copy of the value to the stack
        /// </summary>
        Dupplicate = 1,
        /// <summary>
        /// currently Returning the value
        /// </summary>
        Return = 2,
    }
}
