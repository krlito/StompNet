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
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StompNet.IO
{
    /// <summary>
    /// Custom asynchronous StreamWriter.
    /// </summary>
    internal sealed class AsyncStreamWriter : IDisposable
    {
        public const int DefaultIniBufferCapacity = 1 << 10;
        public const int DefaultMaxBufferCapacity = 1 << 20;

        private byte[] _buffer;
        private int _count;
        private readonly int _maxBufferCapacity;

        private readonly Stream _stream;
        private readonly Encoder _encoder;
        
        public AsyncStreamWriter(Stream stream, Encoding encoding, int initialBufferCapacity = DefaultIniBufferCapacity, int maxBufferCapacity = DefaultMaxBufferCapacity)
        {
            if (initialBufferCapacity < 4)
                throw new ArgumentOutOfRangeException("initialBufferCapacity");

            if (initialBufferCapacity > maxBufferCapacity)
                maxBufferCapacity = initialBufferCapacity;

            _stream = stream;
            _buffer = new byte[initialBufferCapacity];
            _maxBufferCapacity = maxBufferCapacity;
            _encoder = encoding.GetEncoder();
        }

        public AsyncStreamWriter(Stream stream, int initialBufferCapacity = DefaultIniBufferCapacity, int maxBufferCapacity = DefaultMaxBufferCapacity)
            : this (stream, Encoding.UTF8, initialBufferCapacity, maxBufferCapacity)
        {
            
        }

        public async Task WriteAsync(byte value, CancellationToken cancellationToken)
        {
            if (await TryPrepareBuffer(1, false, cancellationToken))
            {
                _buffer[_count] = value;
                _count++;
            }
            else
            {
                _stream.WriteByte(value);
            }
        }

        public async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (await TryPrepareBuffer(count, false, cancellationToken))
            {
                Buffer.BlockCopy(buffer, offset, _buffer, _count, count);
                _count += count;
            }
            else
            {
                await _stream.WriteAsync(buffer, 0, buffer.Length, cancellationToken);
            }
        }

        public Task WriteAsync(byte[] buffer, CancellationToken cancellationToken)
        {
            return WriteAsync(buffer, 0, buffer.Length, cancellationToken);
        }

        public async Task WriteAsync(char[] chars, int charOffset, int charCount, CancellationToken cancellationToken)
        {
            bool completed;
            int charsUsed, bytesUsed;

            _encoder.Convert(chars, 0, chars.Length, _buffer, _count, _buffer.Length - _count, false, out charsUsed, out bytesUsed, out completed);
            _count += bytesUsed;

            while (!completed)
            {
                try
                {
                    await TryPrepareBuffer(_buffer.Length * 2, true, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    _encoder.Reset();
                    throw;
                }

                _encoder.Convert(chars, 0, chars.Length, _buffer, _count, _buffer.Length - _count, false, out charsUsed, out bytesUsed, out completed);
                _count += bytesUsed;
            }

            _encoder.Reset();
        }

        public Task WriteAsync(char[] chars, CancellationToken cancellationToken)
        {
            return WriteAsync(chars, 0, chars.Length, cancellationToken);
        }

        public Task WriteAsync(string str, int strOffset, int strCount, CancellationToken cancellationToken)
        {
            char[] chars = str.ToCharArray(strOffset, strCount);
            return WriteAsync(chars, 0, chars.Length, cancellationToken);
        }

        public Task WriteAsync(string str, CancellationToken cancellationToken)
        {
            return WriteAsync(str, 0, str.Length, cancellationToken);
        }

        public async Task FlushAsync(CancellationToken cancellationToken)
        {
            await _stream.WriteAsync(_buffer, 0, _count, cancellationToken);
            _count = 0;

            await _stream.FlushAsync(cancellationToken);
        }

        private async Task<bool> TryPrepareBuffer(int size, bool toMaxSizeWhenFailed, CancellationToken cancellationToken)
        {
            if (_count + size <= _buffer.Length)
            {
                return true;
            }

            if (_count + size <= _maxBufferCapacity)
            {
                int newLength = _buffer.Length * 2;
                while (newLength < _count + size && newLength < _maxBufferCapacity)
                    newLength *= 2;

                if (newLength > _maxBufferCapacity)
                    newLength = _maxBufferCapacity;

                Array.Resize(ref _buffer, newLength);

                return true;
            }

            if (size <= _maxBufferCapacity)
            {
                await FlushAsync(cancellationToken);
                return await TryPrepareBuffer(size, toMaxSizeWhenFailed, cancellationToken);
            }

            await FlushAsync(cancellationToken);

            if (toMaxSizeWhenFailed && _buffer.Length < _maxBufferCapacity)
            {
                _buffer = new byte[_maxBufferCapacity];
                //Array.Resize(ref _buffer, _maxBufferCapacity);
            }

            return false;
        }

        public void Dispose()
        {
            
        }
    }
}
