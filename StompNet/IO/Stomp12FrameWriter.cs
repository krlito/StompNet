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

using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using StompNet.Models;
using StompNet.Models.Frames;

namespace StompNet.IO
{
    /// <summary>
    /// Frame writer for Stomp 1.2.
    /// 
    /// This class is not thread-safe.
    /// For thread-safety use StompSerialFrameWriter as a wrapper of this one.
    /// </summary>
    public sealed class Stomp12FrameWriter : IStompFrameWriter
    {
        private readonly AsyncStreamWriter _writer;
        private readonly bool _writerOwner;

        public string ProtocolVersion
        {
            get { return "1.2"; }
        }

        internal Stomp12FrameWriter(AsyncStreamWriter writer)
        {
            _writer = writer;
            _writerOwner = false;
        }

        /// <summary>
        /// Constructor for a STOMP 1.2 frame writer.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="initialBufferCapacity">Initial write buffer capacity.</param>
        /// <param name="maxBufferCapacity">Maximum write buffer capacity.</param>
        public Stomp12FrameWriter(Stream stream, int initialBufferCapacity = AsyncStreamWriter.DefaultIniBufferCapacity, int maxBufferCapacity = AsyncStreamWriter.DefaultMaxBufferCapacity)
        {
            _writer = new AsyncStreamWriter(stream, Encoding.UTF8, initialBufferCapacity, maxBufferCapacity);
            _writerOwner = true;
        }

        public async Task WriteAsync(Frame frame, CancellationToken cancellationToken)
        {
            //Make sure IO operations do not get executed on UI thread.
            await WriteFrameAsyncImpl(frame, cancellationToken).ConfigureAwait(false);
        }

        private async Task WriteFrameAsyncImpl(Frame frame, CancellationToken cancellationToken)
        {
#if TRACE
            Console.WriteLine(frame);
#endif
            if (frame.Command == StompCommands.Connect)
            {
                if (frame.Headers.Any(kvp => kvp.Key.Any(ch => ch == '\n' || ch == ':') || kvp.Value.Any(ch => ch == '\n')))
                    throw new InvalidDataException("Connect header names MUST not contain LF or ':' characters; Connected header values MUST not contain any LF.");

                await _writer.WriteAsync(frame.Command, cancellationToken);
                await _writer.WriteAsync(StompOctets.LineFeedByte, cancellationToken);

                await WriteConnectHeadersAsync(frame, cancellationToken);

                await _writer.WriteAsync(frame.BodyArray, cancellationToken);
                await _writer.WriteAsync(StompOctets.EndOfFrameByte, cancellationToken);
            }
            else if (frame.Command == StompCommands.Heartbeat)
            {
                await _writer.WriteAsync(StompOctets.LineFeedByte, cancellationToken);
            }
            else
            {
                await _writer.WriteAsync(frame.Command, cancellationToken);
                await _writer.WriteAsync(StompOctets.LineFeedByte, cancellationToken);

                await WriteHeadersAsync(frame, cancellationToken);

                await _writer.WriteAsync(frame.BodyArray, cancellationToken);
                await _writer.WriteAsync(StompOctets.EndOfFrameByte, cancellationToken);
            }

            await _writer.FlushAsync(cancellationToken);
        }
        
        private async Task WriteConnectHeadersAsync(Frame frame, CancellationToken cancellationToken)
        {
            foreach (var header in frame.Headers.Where(header => header.Value != null))
            {
                await _writer.WriteAsync(header.Key, cancellationToken);
                await _writer.WriteAsync(StompOctets.ColonByte, cancellationToken);
                await _writer.WriteAsync(header.Value, cancellationToken);
                await _writer.WriteAsync(StompOctets.LineFeedByte, cancellationToken);
            }
            await _writer.WriteAsync(StompOctets.LineFeedByte, cancellationToken);
        }

        private async Task WriteHeadersAsync(Frame frame, CancellationToken cancellationToken)
        {
            foreach (var header in frame.Headers.Where(header => header.Value != null))
            {
                await WriteEscapedString(header.Key, cancellationToken);
                await _writer.WriteAsync(StompOctets.ColonByte, cancellationToken);
                await WriteEscapedString(header.Value, cancellationToken);
                await _writer.WriteAsync(StompOctets.LineFeedByte, cancellationToken);
            }
            await _writer.WriteAsync(StompOctets.LineFeedByte, cancellationToken);
        }

        private async Task WriteEscapedString(string str, CancellationToken cancellationToken)
        {
            int cue = 0;
            for(int i = 0; i < str.Length; i++)
            {
                byte[] escapedChar = StompOctets.EscapeOctet(str[i]);

                if (escapedChar != null)
                {
                    await _writer.WriteAsync(str, cue, i - cue, cancellationToken);
                    await _writer.WriteAsync(escapedChar, cancellationToken);
                    cue = i + 1;
                }
            }
            await _writer.WriteAsync(str, cue, str.Length - cue, cancellationToken);
        }

        public void Dispose()
        {
            if (_writerOwner)
                _writer.Dispose();
        }
    }
}
