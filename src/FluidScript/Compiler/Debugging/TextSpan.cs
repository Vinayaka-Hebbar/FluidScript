﻿using System;

namespace FluidScript.Compiler.Debugging
{
    public
#if LATEST_VS
        readonly
#endif
        struct TextSpan
    {
        /// <summary>
        /// Creates a new SourceCodeSpan instance.
        /// </summary>
        /// <param name="startLine"> The start line of this SourceCodeSpan. Must be greater than
        /// zero. </param>
        /// <param name="startColumn"> The start column of this SourceCodeSpan. Must be greater
        /// than zero. </param>
        /// <param name="endLine"> The end line of this SourceCodeSpan. Must be greater than
        /// <paramref name="startLine"/>. </param>
        /// <param name="endColumn"> The end column of this SourceCodeSpan. Must be greater than
        /// <paramref name="startColumn"/>. </param>
        public TextSpan(int startLine, int startColumn, int endLine, int endColumn)
        {
            if (startLine < 1)
                throw new ArgumentOutOfRangeException("startLine");
            if (startColumn < 1)
                throw new ArgumentOutOfRangeException("startColumn");
            if (endLine < startLine)
                throw new ArgumentOutOfRangeException("endLine");
            if (endColumn < startColumn)
                throw new ArgumentOutOfRangeException("endColumn");

            StartLine = startLine;
            StartColumn = startColumn;
            EndLine = endLine;
            EndColumn = endColumn;
        }

        /// <summary>
        /// Creates a new SourceCodeSpan instance.
        /// </summary>
        /// <param name="start"> The start line and column of this SourceCodeSpan. </param>
        /// <param name="end"> The end line and column of this SourceCodeSpan. </param>
        public TextSpan(TextPosition start, TextPosition end)
        {
            StartLine = start.Line;
            StartColumn = start.Column;
            EndLine = end.Line;
            EndColumn = end.Column;
        }

        /// <summary>
        /// Gets the starting line number of this range.
        /// </summary>
        public int StartLine
        {
            get;
        }

        /// <summary>
        /// Gets the starting column number of this range.
        /// </summary>
        public int StartColumn
        {
            get;
        }

        /// <summary>
        /// Gets the ending line number of this range.
        /// </summary>
        public int EndLine
        {
            get;
        }

        /// <summary>
        /// Gets the ending column number of this range.
        /// </summary>
        public int EndColumn
        {
            get;
        }
    }
}
