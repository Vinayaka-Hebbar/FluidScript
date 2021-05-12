namespace FluidScript.Compiler
{
    public class TextSource : ITextSource
    {
        readonly string _text;
        readonly int length;
        int pos;
        int column;
        //default 1
        int line;

        public TextSource(string text)
        {
            _text = text;
            length = text.Length;
            column = line = 1;
            pos = 0;
        }

        public long Position => pos;

        public long Length => length;

        public bool CanAdvance => pos < length;

        public string Path => null;

        public Debugging.TextPosition LineInfo => new Debugging.TextPosition(line, column);

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
            column = 1;
        }

        public void NextLine()
        {
            column = 1;
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

        public char FallBack()
        {
            return _text[--pos];
        }

        public void SkipTo(char c)
        {
            char n;
            for (; ; )
            {
                if (pos >= length)
                    return;
                n = _text[pos++];
                if (n == c)
                    break;
                pos++;
                if (n == '\r' && PeekChar() == '\n')
                {
                    pos++;
                    NextLine();
                    continue;
                }
                column++;
            }
        }

        /// <inheritdoc/>
        public void Skip(char[] c)
        {
            for (; ; )
            {
                if (pos >= length)
                    return;
                char n = _text[pos++];
                if (!System.Array.Exists(c, value => value == n))
                    break;
                pos++;
                if (n == '\r' && PeekChar() == '\n')
                {
                    pos++;
                    NextLine();
                    continue;
                }
                column++;
            }
        }
    }
}
