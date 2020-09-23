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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using StompNet.Helpers;
using StompNet.Models;
using StompNet.Models.Frames;

namespace StompNet.IO
{
    /// <summary>
    /// IStompFrameWriter extensions to write STOMP commands in a stream.
    /// </summary>
    public static class StompFrameWriterExtensions
    {
        public static Task WriteConnectAsync(
            this IStompFrameWriter writer,
            string host = null,
            string login = null,
            string passcode = null,
            Heartbeat heartbeat = null,
            IEnumerable<KeyValuePair<string, string>> extraHeaders = null,
            CancellationToken? cancellationToken = null)
        {
            CancellationToken cancelToken = cancellationToken ?? CancellationToken.None;
            Frame frame = StompFrameFactory.CreateConnect(writer.ProtocolVersion, host, login, passcode, heartbeat, extraHeaders);
            return writer.WriteAsync(frame, cancelToken);
        }

        public static Task WriteSendAsync(
            this IStompFrameWriter writer,
            string destination,
            byte[] body = null,
            string contentType = MediaTypeNames.ApplicationOctet,
            string receipt = null,
            string transaction = null,
            IEnumerable<KeyValuePair<string, string>> extraHeaders = null,
            CancellationToken? cancellationToken = null)
        {
            CancellationToken cancelToken = cancellationToken ?? CancellationToken.None;
            Frame frame = StompFrameFactory.CreateSend(destination, body, contentType, receipt, transaction, extraHeaders);
            return writer.WriteAsync(frame, cancelToken);
        }

        public static Task WriteSendAsync(
            this IStompFrameWriter writer,
            string destination,
            string body,
            Encoding encoding,
            string receipt = null,
            string transaction = null,
            IEnumerable<KeyValuePair<string, string>> extraHeaders = null,
            CancellationToken? cancellationToken = null)
        {
            CancellationToken cancelToken = cancellationToken ?? CancellationToken.None;
            Frame frame = StompFrameFactory.CreateSend(destination, body, encoding, receipt, transaction, extraHeaders);
            return writer.WriteAsync(frame, cancelToken);
        }

        public static Task WriteSendAsync(
            this IStompFrameWriter writer,
            string destination,
            string body,
            string receipt = null,
            string transaction = null,
            IEnumerable<KeyValuePair<string, string>> extraHeaders = null,
            CancellationToken? cancellationToken = null)
        {
            CancellationToken cancelToken = cancellationToken ?? CancellationToken.None;
            Frame frame = StompFrameFactory.CreateSend(destination, body, Encoding.UTF8, receipt, transaction, extraHeaders);
            return writer.WriteAsync(frame, cancelToken);
        }

        public static Task WriteSubscribeAsync(
            this IStompFrameWriter writer,
            string destination,
            string id,
            string receipt = null,
            string ack = null,
            IEnumerable<KeyValuePair<string, string>> extraHeaders = null,
            CancellationToken? cancellationToken = null)
        {
            CancellationToken cancelToken = cancellationToken ?? CancellationToken.None;
            Frame frame = StompFrameFactory.CreateSubscribe(destination, id, receipt, ack, extraHeaders);
            return writer.WriteAsync(frame, cancelToken);
        }

        public static Task WriteUnsubscribeAsync(
            this IStompFrameWriter writer,
            string id,
            string receipt = null,
            IEnumerable<KeyValuePair<string, string>> extraHeaders = null,
            CancellationToken? cancellationToken = null)
        {
            CancellationToken cancelToken = cancellationToken ?? CancellationToken.None;
            Frame frame = StompFrameFactory.CreateUnsubscribe(id, receipt, extraHeaders);
            return writer.WriteAsync(frame, cancelToken);
        }

        public static Task WriteAckAsync(
            this IStompFrameWriter writer,
            string id,
            string receipt = null,
            string transaction = null,
            IEnumerable<KeyValuePair<string, string>> extraHeaders = null,
            CancellationToken? cancellationToken = null)
        {
            CancellationToken cancelToken = cancellationToken ?? CancellationToken.None;
            Frame frame = StompFrameFactory.CreateAck(id, receipt, transaction, extraHeaders);
            return writer.WriteAsync(frame, cancelToken);
        }

        public static Task WriteNackAsync(
            this IStompFrameWriter writer,
            string id,
            string receipt = null,
            string transaction = null,
            IEnumerable<KeyValuePair<string, string>> extraHeaders = null,
            CancellationToken? cancellationToken = null)
        {
            CancellationToken cancelToken = cancellationToken ?? CancellationToken.None;
            Frame frame = StompFrameFactory.CreateNack(id, receipt, transaction, extraHeaders);
            return writer.WriteAsync(frame, cancelToken);
        }

        public static Task WriteBeginAsync(
            this IStompFrameWriter writer,
            string transaction,
            string receipt = null,
            IEnumerable<KeyValuePair<string, string>> extraHeaders = null,
            CancellationToken? cancellationToken = null)
        {
            CancellationToken cancelToken = cancellationToken ?? CancellationToken.None;
            Frame frame = StompFrameFactory.CreateBegin(transaction, receipt, extraHeaders);
            return writer.WriteAsync(frame, cancelToken);
        }

        public static Task WriteCommitAsync(
            this IStompFrameWriter writer,
            string transaction,
            string receipt = null,
            IEnumerable<KeyValuePair<string, string>> extraHeaders = null,
            CancellationToken? cancellationToken = null)
        {
            CancellationToken cancelToken = cancellationToken ?? CancellationToken.None;
            Frame frame = StompFrameFactory.CreateCommit(transaction, receipt, extraHeaders);
            return writer.WriteAsync(frame, cancelToken);
        }

        public static Task WriteAbortAsync(
            this IStompFrameWriter writer,
            string transaction,
            string receipt = null,
            IEnumerable<KeyValuePair<string, string>> extraHeaders = null,
            CancellationToken? cancellationToken = null)
        {
            CancellationToken cancelToken = cancellationToken ?? CancellationToken.None;
            Frame frame = StompFrameFactory.CreateAbort(transaction, receipt, extraHeaders);
            return writer.WriteAsync(frame, cancelToken);
        }

        public static Task WriteDisconnectAsync(
            this IStompFrameWriter writer,
            string receipt = null,
            IEnumerable<KeyValuePair<string, string>> extraHeaders = null,
            CancellationToken? cancellationToken = null)
        {
            CancellationToken cancelToken = cancellationToken ?? CancellationToken.None;
            Frame frame = StompFrameFactory.CreateDisconnect(receipt, extraHeaders);
            return writer.WriteAsync(frame, cancelToken);
        }
    }
}
