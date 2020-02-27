namespace FluidScript.Compiler.Debugging
{
    public
#if LATEST_VS
        readonly
#endif
        struct TextPosition
    {
        /// <summary>
        /// Creates a new SourceCodePosition instance.
        /// </summary>
        /// <param name="line"> The line number. Must be greater than zero. </param>
        /// <param name="column"> The column number. Must be greater than zero. </param>
        public TextPosition(int line, int column)
            : this()
        {
            if (line < 1)
                throw new System.ArgumentOutOfRangeException("line");
            if (column < 1)
                throw new System.ArgumentOutOfRangeException("column");

            this.Line = line;
            this.Column = column;
        }

        /// <summary>
        /// Gets the line number.
        /// </summary>
        public int Line
        {
            get;
        }

        /// <summary>
        /// Gets the column number.
        /// </summary>
        public int Column
        {
            get;
        }

        public override string ToString()
        {
            return string.Concat("line = ", Line, ", column = ", Column);
        }
    }
}