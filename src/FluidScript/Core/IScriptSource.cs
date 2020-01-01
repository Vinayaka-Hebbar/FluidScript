using FluidScript.Compiler;
using System;

namespace FluidScript.Library
{
    public interface IScriptSource : IDisposable
    {
        string Path { get; }

        long Position { get; }

        long Length { get; }

        bool CanAdvance { get; }

        TextPosition CurrentPosition { get; }

        char ReadChar();

        char PeekChar();

        void Reset();

        void NextLine();

        void SeekTo(long pos);

        void FallBack();
    }
}
