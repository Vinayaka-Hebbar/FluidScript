using System;

namespace FluidScript.Compiler
{
    public interface ITextSource : IDisposable
    {
        string Path { get; }

        long Position { get; }

        long Length { get; }

        bool CanAdvance { get; }

        Debugging.TextPosition LineInfo { get; }

        /// <summary>
        /// Read next char and advance pos to next
        /// </summary>
        char ReadChar();

        /// <summary>
        /// Peek next char
        /// </summary>
        char PeekChar();

        void Reset();

        void NextLine();

        void SeekTo(long pos);

        /// <summary>
        /// Skip current pos to char <paramref name="c"/>
        /// </summary>
        /// <param name="c">character to move pos</param>
        void SkipTo(char c);
        /// <summary>
        /// Skip the specified <paramref name="c"/> values
        /// </summary>
        /// <param name="c">the charecters to skip</param>
        void Skip(char[] c);

        /// <summary>
        /// Revert to pos-1
        /// </summary>
        /// <returns>Current Char in the position</returns>
        char FallBack();
    }
}
