namespace FluidScript.Compiler.Emit
{
    public enum MethodCompileOption
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
        /// <summary>
        /// Calling Argument has this
        /// </summary>
        HasThis = 4,
        /// <summary>
        /// Emit address of start
        /// </summary>
        EmitStartAddress = 8


    }
}
