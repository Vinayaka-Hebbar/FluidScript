namespace FluidScript.Runtime
{
    public enum RegisterImplOption
    {
        /// <summary>
        /// No Implementation
        /// </summary>
        Default = 0,
        /// <summary>
        /// can be user to create object
        /// </summary>
        Activator = 2,
        /// <summary>
        /// Library type of implementation
        /// </summary>
        Library = 8
    }
}
