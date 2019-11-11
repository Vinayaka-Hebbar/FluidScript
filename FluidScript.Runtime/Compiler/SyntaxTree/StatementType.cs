namespace FluidScript.Compiler.SyntaxTree
{
    public enum StatementType
    {
        Empty = 0,
        Declaration = 15,
        Block = 16,
        Function = 17,
        Expression = 36,
        Return = 37,
        Throw = 38,
        If = 39,
        Do = 42,
        /// <summary>
        /// for and while
        /// </summary>
        Loop = 43,
        Class = 44,
    }
}
