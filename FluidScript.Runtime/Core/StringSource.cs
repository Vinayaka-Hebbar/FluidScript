using FluidScript.Compiler;

namespace FluidScript.Core
{
    public sealed class StringSource : IScriptSource
    {
        private readonly string _text;
        private readonly int length;
        private int pos;
        private int column = 1;
        //default 1
        private int line = 1;

        public StringSource(string text)
        {
            _text = text;
            length = text.Length;
        }

        public long Position => pos;

        public long Length => length;

        public bool CanAdvance => pos < length;

        public string Path => null;

        public TextPosition CurrentPosition => new TextPosition(line, column);

        void System.IDisposable.Dispose()
        {
            System.GC.SuppressFinalize(this);
        }
        public char ReadChar()
        {
            if (pos >= length)
                return char.MinValue;
            column++;
            return _text[pos++];
        }

        public char PeekChar()
        {
            if (pos < length)
                return _text[pos];
            return char.MinValue;
        }


        public void Reset()
        {
            pos = 0;
            line = 1;
            column = 0;
        }

        public void NextLine()
        {
            column = 0;
            line++;
        }

        public void SeekTo(long pos)
        {
            this.pos = (int)pos;
        }

        public override string ToString()
        {
            return _text;
        }

        public void FallBack()
        {
            pos--;
        }
    }
}
