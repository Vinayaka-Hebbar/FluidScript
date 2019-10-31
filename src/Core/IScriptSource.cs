using System;

namespace FluidScript.Core
{
    public interface IScriptSource : IDisposable
    {
        string Path { get; }

        long Position { get; }

        int Column { get; }

        int Line { get; }

        long Length { get; }

        bool CanAdvance { get; }

        char ReadChar();

        char PeekChar();

        void Reset();

        void NextLine();

        void SeekTo(long pos);

        char FallBack();
    }
}
