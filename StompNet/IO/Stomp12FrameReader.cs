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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using StompNet.Helpers;
using StompNet.Models;
using StompNet.Models.Frames;

namespace StompNet.IO
{
    /// <summary>
    /// Frame reader for Stomp 1.2.
    /// 
    /// This class is not thread-safe.
    /// For thread-safety use StompSerialFrameReader as a wrapper of this one.
    /// </summary>
    public sealed class Stomp12FrameReader : IStompFrameReader
    {
        private readonly AsyncStreamReader _reader;
        private readonly bool _readerOwner;

        public string ProtocolVersion
        {
            get { return "1.2"; }
        }

        internal Stomp12FrameReader(AsyncStreamReader reader)
        {
            _reader = reader;
            _readerOwner = false;
        }

        /// <summary>
        /// Constructor for a STOMP 1.2 frame reader.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="bufferCapacity">The read buffer size.</param>
        public Stomp12FrameReader(Stream stream, int? bufferCapacity = null)
        {
            _reader = new AsyncStreamReader(stream, Encoding.UTF8, bufferCapacity);
            _readerOwner = true;
        }

        public async Task<Frame> ReadFrameAsync(CancellationToken cancellationToken)
        {
            //Make sure IO operations do not get executed on UI thread.
            Frame frame = await ReadFrameAsyncImpl(cancellationToken).ConfigureAwait(false);
#if TRACE
            Console.WriteLine(frame);
#endif
            return frame;
        }

        private async Task<Frame> ReadFrameAsyncImpl(CancellationToken cancellationToken)
        {
            //Read command.
            string command = await ReadCommandAsync(cancellationToken);

            if (command == string.Empty)
                return Frame.HeartbeatFrame;

            //Read headers.
            IEnumerable<KeyValuePair<string, string>> headers;
            if(command == StompCommands.Connected)
                headers = await ReadConnectedHeaders(cancellationToken);
            else 
                headers = await ReadHeaders(cancellationToken);

            //Read content.
            int contentLength = GetContentLengthFromHeaders(headers);
            byte[] body = await ReadBodyAsync(contentLength, cancellationToken);

            return new Frame(command, headers, body);
        }

        private async Task<string> ReadCommandAsync(CancellationToken cancellationToken)
        {
            string command = await _reader.ReadLineAsync(cancellationToken);

            if (command != string.Empty && !StompCommands.IsServerCommand(command))
                throw new InvalidDataException(string.Format("'{0}' is not a valid STOMP server command.", command));

            return command;
        }

        private async Task<IEnumerable<KeyValuePair<string, string>>> ReadConnectedHeaders(CancellationToken cancellationToken)
        {
            var headers = new List<KeyValuePair<string, string>>();

            while (true)
            {
                var read = await _reader.ReadStringUntilAsync(new[] { ':', '\n' }, cancellationToken);

                if (read.TargetChar == '\n')
                {
                    if (read.String.Length == 0)
                    {
                        break;
                    }
                    else if (read.String.Length == 1 && read.String[0] == '\r')
                    {
                        throw new InvalidDataException("Connected header line containing only a carriage return.");
                    }
                    else
                    {
                        throw new InvalidDataException("Connected header name without value.");
                    }
                }

                if (read.String.Length == 0)
                {
                    throw new InvalidDataException("Connected header names MUST have one character at least.");
                }

                string headerName = read.String;
                string headerValue = await _reader.ReadStringUntilAsync('\n', cancellationToken);

                headers.Add(new KeyValuePair<string, string>(headerName, headerValue));
            }

            return headers;
        }

        private async Task<IEnumerable<KeyValuePair<string, string>>> ReadHeaders(CancellationToken cancellationToken)
        {
            StringBuilder stringBuilder = new StringBuilder();
            var headers = new List<KeyValuePair<string, string>>();

            while (true)
            {
                // Get Header Name
                char ch = await _reader.ReadCharAsync(cancellationToken);
                if (ch == '\n')
                {
                    break;
                }
                else if (ch == '\r')
                {
                    if (await _reader.ReadCharAsync(cancellationToken) == '\n')
                    {
                        break;
                    }
                    throw new InvalidDataException("Header data MUST not contain CR, LF or ':' characters.");
                }

                stringBuilder.Clear();

                while (ch != ':')
                {
                    if (ch == '\n' || ch == '\r')
                        throw new InvalidDataException("Headers data MUST not contain CR, LF or ':' characters.");
                    else if(ch == '\\')
                        stringBuilder.Append(StompOctets.UnescapeOctet(await _reader.ReadCharAsync(cancellationToken)));
                    else
                        stringBuilder.Append(ch);

                    ch = await _reader.ReadCharAsync(cancellationToken);
                }

                string headerName = stringBuilder.ToString();

                // Get Header Value
                ch = await _reader.ReadCharAsync(cancellationToken);
                stringBuilder.Clear();

                while (ch != '\n' && ch != '\r')
                {
                    if(ch == ':')
                        throw new InvalidDataException("Headers data MUST not contain CR, LF or ':' characters.");
                    else if(ch == '\\')
                        stringBuilder.Append(StompOctets.UnescapeOctet(await _reader.ReadCharAsync(cancellationToken)));
                    else
                        stringBuilder.Append(ch);

                    ch = await _reader.ReadCharAsync(cancellationToken);
                }

                if(ch == '\r' && await _reader.ReadCharAsync(cancellationToken) != '\n')
                    throw new InvalidDataException("Header data MUST not contain CR, LF or ':' characters.");

                string headerValue = stringBuilder.ToString();

                headers.Add(new KeyValuePair<string, string>(headerName, headerValue));
            }

            return headers;
        }

        private static int GetContentLengthFromHeaders(IEnumerable<KeyValuePair<string, string>> headers)
        {
            int contentLength = -1;

            headers
                .Where(header => header.Key == StompHeaders.ContentLength)
                .Select(header => header.Value)
                .Take(1)
                .Do(
                    value =>
                    {
                        if (!int.TryParse(value, out contentLength) || contentLength < 0)
                            throw new InvalidDataException(StompHeaders.ContentLength + " must be a positive integer.");
                    }
                );

            return contentLength;
        }

        private async Task<byte[]> ReadBodyAsync(int contentLength, CancellationToken cancellationToken)
        {
            byte[] body;

            if (contentLength == -1)
            {
                body = await _reader.ReadBytesUntilAsync(0, cancellationToken);
            }
            else
            {
                body = new byte[contentLength];

                await _reader.ReadNBytesAsync(body, 0, contentLength, cancellationToken);

                byte b = await _reader.ReadByteAsync(cancellationToken);
                if (b != 0)
                    throw new InvalidDataException("Body is not NULL terminated.");
            }

            return body;
        }

        public void Dispose()
        {
            if(_readerOwner)
                _reader.Dispose();
        }
    }
}
