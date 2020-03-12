using System;

namespace FluidScript.Compiler
{
    public interface IScriptSource : IDisposable
    {
        string Path { get; }

        long Position { get; }

        long Length { get; }

        bool CanAdvance { get; }

        Compiler.Debugging.TextPosition CurrentPosition { get; }

        /// <summary>
        /// Read next char
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
        /// Revert to pos-1
        /// </summary>
        /// <returns>Current Char in the position</returns>
        char FallBack();
    }
}
