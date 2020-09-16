using System;
using System.IO;
using System.Linq;
using System.Text;

namespace FluidScript.Compiler
{
    public class StreamSource : ITextSource
    {
        private int column = 1;
        private int line = 1;
        private bool _detectEncoding;

        private readonly Stream _stream;

        private int charLen;

        private long charPos;

        private readonly byte[] _preamble;

        private char[] charBuffer;

        private int _maxCharsPerBuffer;

        private Decoder decoder;

        private Encoding encoding;

        private int byteLen;

        private bool _isBlocked;

        private int bytePos;

        private bool _checkPreamble;

        private readonly byte[] byteBuffer;

        private readonly int bufferSize;

        internal StreamSource(FileInfo info)
        {
            Path = info.FullName;
            _stream = info.Open(FileMode.Open);
            bufferSize = 1024;
            encoding = Encoding.UTF8;
            decoder = encoding.GetDecoder();
            byteBuffer = new byte[bufferSize];
            _maxCharsPerBuffer = encoding.GetMaxCharCount(bufferSize);
            charBuffer = new char[_maxCharsPerBuffer];
            byteLen = 0;
            bytePos = 0;
            _detectEncoding = true;
            _preamble = encoding.GetPreamble();
            _checkPreamble = (_preamble.Length > 0);
            _isBlocked = false;
        }

        public StreamSource(Stream stream)
        {
            _stream = stream;
            bufferSize = 1024;
            encoding = Encoding.UTF8;
            decoder = encoding.GetDecoder();
            byteBuffer = new byte[bufferSize];
            _maxCharsPerBuffer = encoding.GetMaxCharCount(bufferSize);
            charBuffer = new char[_maxCharsPerBuffer];
            byteLen = 0;
            bytePos = 0;
            _detectEncoding = true;
            _preamble = encoding.GetPreamble();
            _checkPreamble = (_preamble.Length > 0);
            _isBlocked = false;
        }

        public string Path { get; }

        public long Position => charPos;

        public long Length => _stream.Length;

        public bool CanAdvance
        {
            get
            {
                if (charPos < charLen)
                    return true;

                // This may block on pipes!
                int numRead = ReadBuffer();
                return numRead > 0;
            }
        }

        public Debugging.TextPosition LineInfo => new Debugging.TextPosition(line, column);

        public void Dispose()
        {
            _stream.Dispose();
        }

        public void NextLine()
        {
            column = 1;
            line++;
        }

        private void CompressBuffer(int n)
        {
            Buffer.BlockCopy(byteBuffer, n, byteBuffer, 0, byteLen - n);
            byteLen -= n;
        }

        private void DetectEncoding()
        {
            if (byteLen < 2)
                return;
            _detectEncoding = false;
            bool changedEncoding = false;
            if (byteBuffer[0] == 0xFE && byteBuffer[1] == 0xFF)
            {
                // Big Endian Unicode

                encoding = new UnicodeEncoding(true, true);
                CompressBuffer(2);
                changedEncoding = true;
            }

            else if (byteBuffer[0] == 0xFF && byteBuffer[1] == 0xFE)
            {
                // Little Endian Unicode, or possibly little endian UTF32
                if (byteLen < 4 || byteBuffer[2] != 0 || byteBuffer[3] != 0)
                {
                    encoding = new UnicodeEncoding(false, true);
                    CompressBuffer(2);
                    changedEncoding = true;
                }
            }

            else if (byteLen >= 3 && byteBuffer[0] == 0xEF && byteBuffer[1] == 0xBB && byteBuffer[2] == 0xBF)
            {
                // UTF-8
                encoding = Encoding.UTF8;
                CompressBuffer(3);
                changedEncoding = true;
            }
            else if (byteLen == 2)
                _detectEncoding = true;
            // Note: in the future, if we change this algorithm significantly,
            // we can support checking for the preamble of the given encoding.

            if (changedEncoding)
            {
                decoder = encoding.GetDecoder();
                _maxCharsPerBuffer = encoding.GetMaxCharCount(byteBuffer.Length);
                charBuffer = new char[_maxCharsPerBuffer];
            }
        }

        // Trims the preamble bytes from the byteBuffer. This routine can be called multiple times
        // and we will buffer the bytes read until the preamble is matched or we determine that
        // there is no match. If there is no match, every byte read previously will be available 
        // for further consumption. If there is a match, we will compress the buffer for the 
        // leading preamble bytes
        private bool IsPreamble()
        {
            if (!_checkPreamble)
                return _checkPreamble;

            int len = (byteLen >= (_preamble.Length)) ? (_preamble.Length - bytePos) : (byteLen - bytePos);

            for (int i = 0; i < len; i++, bytePos++)
            {
                if (byteBuffer[bytePos] != _preamble[bytePos])
                {
                    bytePos = 0;
                    _checkPreamble = false;
                    break;
                }
            }

            if (_checkPreamble)
            {
                if (bytePos == _preamble.Length)
                {
                    // We have a match
                    CompressBuffer(_preamble.Length);
                    bytePos = 0;
                    _checkPreamble = false;
                    _detectEncoding = false;
                }
            }

            return _checkPreamble;
        }

        private int ReadBuffer()
        {
            charLen = 0;
            charPos = 0;
            if (!_checkPreamble)
                byteLen = 0;
            do
            {
                if (_checkPreamble)
                {
                    int len = _stream.Read(byteBuffer, bytePos, byteBuffer.Length - bytePos);

                    if (len == 0)
                    {
                        // EOF but we might have buffered bytes from previous 
                        // attempt to detect preamble that needs to be decoded now
                        if (byteLen > 0)
                        {
                            charLen += decoder.GetChars(byteBuffer, 0, byteLen, charBuffer, charLen);
                            // Need to zero out the byteLen after we consume these bytes so that we don't keep infinitely hitting this code path
                            bytePos = byteLen = 0;
                        }

                        return charLen;
                    }

                    byteLen += len;
                }
                else
                {
                    byteLen = _stream.Read(byteBuffer, 0, byteBuffer.Length);

                    if (byteLen == 0)  // We're at EOF
                        return charLen;
                }

                // _isBlocked == whether we read fewer bytes than we asked for.
                // Note we must check it here because CompressBuffer or 
                // DetectEncoding will change byteLen.
                _isBlocked = (byteLen < byteBuffer.Length);

                // Check for preamble before detect encoding. This is not to override the
                // user suppplied Encoding for the one we implicitly detect. The user could
                // customize the encoding which we will loose, such as ThrowOnError on UTF8
                if (IsPreamble())
                    continue;

                // If we're supposed to detect the encoding and haven't done so yet,
                // do it.  Note this may need to be called more than once.
                if (_detectEncoding && byteLen >= 2)
                    DetectEncoding();

                charLen += decoder.GetChars(byteBuffer, 0, byteLen, charBuffer, charLen);
            } while (charLen == 0);
            //Console.WriteLine("ReadBuffer called.  chars: "+charLen);
            return charLen;
        }

        public char ReadChar()
        {
            if (charPos == charLen)
            {
                if (ReadBuffer() == 0)
                {
                    return char.MinValue;
                }
            }
            char result = charBuffer[charPos];
            charPos++;
            column++;
            return result;
        }

        public char PeekChar()
        {
            if (charPos == charLen)
            {
                if (_isBlocked || ReadBuffer() == 0)
                    return char.MinValue;
            }
            return charBuffer[charPos];
        }

        public char FallBack()
        {
            if (charPos == 0)
            {
                // whether fallback discard ok?
                Discard();
                return charBuffer[charPos];
            }
            column--;
            charPos--;
            return charBuffer[charPos];
        }

        public void Reset()
        {
            _stream.Seek(0, SeekOrigin.Begin);
            line = 1;
            column = 0;
            Discard();
        }

        public void SeekTo(long pos)
        {
            //todo seek if pos exceeds below range
            charPos = pos;
            column = (int)pos;
        }

        private void Discard()
        {
            byteLen = 0;
            charLen = 0;
            charPos = 0;
            // in general we'd like to have an invariant that encoding isn't null. However,
            // for startup improvements for NullStreamReader, we want to delay load encoding. 
            if (encoding != null)
            {
                decoder = encoding.GetDecoder();
            }
            _isBlocked = false;
        }

        public void SkipTo(char c)
        {
            for (; ; )
            {
                if (charPos == charLen)
                {
                    if (ReadBuffer() == 0)
                    {
                        return;
                    }
                }
                char n = charBuffer[charPos++];
                if (n == c)
                    break;
                if (n == '\r' && PeekChar() == '\n')
                {
                    charPos++;
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
                if (charPos == charLen)
                {
                    if (ReadBuffer() == 0)
                    {
                        return;
                    }
                }
                char n = charBuffer[charPos++];
                if (!c.Any(value => value == n))
                    break;
                charPos++;
                if (n == '\r' && PeekChar() == '\n')
                {
                    charPos++;
                    NextLine();
                    continue;
                }
                column++;
            }
        }
    }
}
