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

using StompNet.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StompNet
{
    /// <summary>
    /// This is an interface that has the basic common communication commands 
    /// that are used by IStompConnection and IStompTransaction. DRY!
    /// </summary>
    public interface IStompConnectionBase
    {
        /// <summary>
        /// Send a message.
        /// </summary>
        /// <param name="destination">Queue/topic name which is destination of the message.</param>
        /// <param name="content">Content of the message.</param>
        /// <param name="contentType">Type of the content.</param>
        /// <param name="useReceipt">Indicates whether to require the service to confirm receipt of the message.</param>
        /// <param name="extraHeaders">Non-standard headers to include in the connect request frame.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task representing the send operation.</returns>
        Task SendAsync(
            string destination,
            byte[] content = null,
            string contentType = MediaTypeNames.ApplicationOctet,
            bool useReceipt = false,
            IEnumerable<KeyValuePair<string, string>> extraHeaders = null,
            CancellationToken? cancellationToken = null);

        /// <summary>
        /// Send a message.
        /// </summary>
        /// <param name="destination">Queue/topic name which is destination of the message.</param>
        /// <param name="content">Content of the message.</param>
        /// <param name="encoding">Encoding used to encode the content of the message.</param>
        /// <param name="useReceipt">Indicates whether to require the service to confirm receipt of the message.</param>
        /// <param name="extraHeaders">Non-standard headers to include in the connect request frame.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task representing the send operation.</returns>
        Task SendAsync(
            string destination,
            string content,
            Encoding encoding,
            bool useReceipt = false,
            IEnumerable<KeyValuePair<string, string>> extraHeaders = null,
            CancellationToken? cancellationToken = null);

        /// <summary>
        /// Send a message.
        /// </summary>
        /// <param name="destination">Name of the queue/topic which is destination of the message.</param>
        /// <param name="content">Content of the message. UTF-8 encoding is used.</param>
        /// <param name="useReceipt">Indicates whether to require the service to confirm receipt of the message.</param>
        /// <param name="extraHeaders">Non-standard headers to include in the connect request frame.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task representing the send operation.</returns>
        Task SendAsync(
            string destination,
            string content,
            bool useReceipt = false,
            IEnumerable<KeyValuePair<string, string>> extraHeaders = null,
            CancellationToken? cancellationToken = null);

        /// <summary>
        /// Subscribe to a queue/topic.
        /// </summary>
        /// <param name="observer">Observer to handle the received messages.</param>
        /// <param name="destination">Name of the queue/topic from which messages are get.</param>
        /// <param name="ack">Type of acknowledgement that is going to be used by the subscription. 
        /// Its value can be: "auto" (default), "client" or "client-individual".</param>
        /// <param name="useReceipt">Indicates whether to require the service to confirm receipt of the message.</param>
        /// <param name="extraHeaders">Non-standard headers to include in the connect request frame.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task representing the subscribe operation. The result of this task is an IAsyncDisposable that may be used to unsubscribe.</returns>
        Task<IAsyncDisposable> SubscribeAsync(
            IObserver<IStompMessage> observer,
            string destination,
            string ack = null,
            bool useReceipt = false,
            IEnumerable<KeyValuePair<string, string>> extraHeaders = null,
            CancellationToken? cancellationToken = null);
    }
}
