/*******************************************************************************
 *
 *  Copyright (c) 2014 Carlos Campo <carlos@campo.com.co>
 *
 *  Licensed under the Apache License, Version 2.0 (the "License");
 *  you may not use this file except in compliance with the License.
 *  You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed under the License is distributed on an "AS IS" BASIS,
 *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  See the License for the specific language governing permissions and
 *  limitations under the License.
 * 
 *******************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StompNet.IO
{
    /// <summary>
    /// Custom asynchronous StreamReader.
    /// </summary>
    internal sealed class AsyncStreamReader : IDisposable
    {
        private readonly bool _bufferedStreamOwner;
        private readonly Stream _stream;
        private readonly Decoder _decoder;
        
        private readonly byte[] _oneByte;
        private readonly char[] _oneChar;
        private readonly MemoryStream _memoryStream;
        private readonly StringBuilder _stringBuilder;

        public AsyncStreamReader(Stream stream, Encoding encoding = null, int? bufferCapacity = null)
        {
            _bufferedStreamOwner = !(stream is BufferedStream) || bufferCapacity.HasValue;
            
            _stream = 
                ! _bufferedStreamOwner ? stream // CAREFUL! This is a negative(!) condition.
                : bufferCapacity == null ? new BufferedStream(stream)
                : new BufferedStream(stream, bufferCapacity.Value);

            _decoder = (encoding ?? Encoding.UTF8).GetDecoder();

            _oneByte = new byte[1];
            _oneChar = new char[1];
            _memoryStream = new MemoryStream();
            _stringBuilder = new StringBuilder();
        }

        // On some cases disposing is propagated faster than cancellation and ObjectDisposedException is generated.
        // This is a helper for these cases.
        private static async Task<T> ThrowIfCancellationRequested<T>(Task<T> task, CancellationToken cancellationToken)
        {
            try
            {
                return await task;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception)
            {
                cancellationToken.ThrowIfCancellationRequested();
                throw;
            }
        }

        public Task<int> ReadBytesAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return ThrowIfCancellationRequested(
                    _stream.ReadAsync(buffer, offset, count, cancellationToken),
                    cancellationToken);
        }

        public async Task ReadNBytesAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            int totalRead = 0;
            while (totalRead < count)
            {
                int bRead = await ReadBytesAsync(buffer, offset + totalRead, count - totalRead, cancellationToken);

                if (bRead == 0)
                {
                    throw new EndOfStreamException();
                }
                totalRead += bRead;
            }
        }

        public async Task<byte> ReadByteAsync(CancellationToken cancellationToken)
        {
            await ReadNBytesAsync(_oneByte, 0, 1, cancellationToken);

            return _oneByte[0];
        }

        public async Task<char> ReadCharAsync(CancellationToken cancellationToken)
        {
            do
            {
                await ReadNBytesAsync(_oneByte, 0, 1, cancellationToken);
            } while (_decoder.GetChars(_oneByte, 0, 1, _oneChar, 0, false) == 0);

            return _oneChar[0];
        }

        public async Task<byte[]> ReadBytesUntilAsync(byte targetByte, CancellationToken cancellationToken)
        {
            _memoryStream.Seek(0, SeekOrigin.Begin);
            _memoryStream.SetLength(0);

            byte b;
            while ((b = await ReadByteAsync(cancellationToken)) != targetByte)
            {
                _memoryStream.WriteByte(b);
            }

            return _memoryStream.ToArray();
        }

        public async Task<ReadStringUntilResult> ReadStringUntilAsync(IEnumerable<char> targetChars, CancellationToken cancellationToken)
        {
            char[] targetCharsArray = targetChars as char[] ?? targetChars.ToArray();
            _stringBuilder.Clear();

            char ch = await ReadCharAsync(cancellationToken);
            while (!targetCharsArray.Contains(ch))
            {
                _stringBuilder.Append(ch);
                ch = await ReadCharAsync(cancellationToken);
            }

            ReadStringUntilResult result;
            result.String = _stringBuilder.ToString();
            result.TargetChar = ch;
            return result;
        }

        public async Task<string> ReadStringUntilAsync(char targetChar, CancellationToken cancellationToken)
        {
            return (await ReadStringUntilAsync(new[] {targetChar}, cancellationToken)).String;
        }

        public async Task<string> ReadLineAsync(CancellationToken cancellationToken)
        {
            _stringBuilder.Clear();
            char ch;

            while ((ch = await ReadCharAsync(cancellationToken)) != '\n')
            {
                _stringBuilder.Append(ch);
            }

            if (_stringBuilder.Length > 0)
            {
                int lastCharPos = _stringBuilder.Length - 1;
                if (_stringBuilder[lastCharPos] == '\r')
                {
                    _stringBuilder.Remove(lastCharPos, 1);
                }
            }

            return _stringBuilder.ToString();
        }

        public void Dispose()
        {
            if(_bufferedStreamOwner)
                _stream.Dispose();
            _memoryStream.Dispose();
        }

        public struct ReadStringUntilResult
        {
            public string String;
            public char TargetChar;
        }
    }
}
