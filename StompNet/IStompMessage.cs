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
using StompNet.Models.Frames;

namespace StompNet
{
    /// <summary>
    /// Stomp message interface.
    /// </summary>
    public interface IStompMessage
    {
        /// <summary>
        /// The received message frame.
        /// </summary>
        MessageFrame MessageFrame { get; }

        /// <summary>
        /// Message content.
        /// </summary>
        IEnumerable<byte> Content { get; }

        /// <summary>
        /// Message content type.
        /// </summary>
        string ContentType { get; }
        
        /// <summary>
        /// Returns true if the message should be acknowledged after processing it.
        /// </summary>
        bool IsAcknowledgeable { get; }

        /// <summary>
        /// Get content as a string using the specified encoding.
        /// </summary>
        /// <param name="encoding">Encoding to be used to decode the message content.</param>
        /// <returns>A string representing the message content decoded using the specified encoding.</returns>
        string GetContentAsString(Encoding encoding);

        /// <summary>
        /// Get content as a string using UTF-8 encoding.
        /// </summary>
        /// <returns>A string representing the message content decoded using UTF-8 encoding.</returns>
        string GetContentAsString();

        /// <summary>
        /// Acknowledge this message.
        /// </summary>
        /// <param name="useReceipt">Indicates whether to require the service to confirm receipt of the message.</param>
        /// <param name="transactionId">The id of the transaction this acknowledge makes part of.</param>
        /// <param name="extraHeaders">Non-standard headers to include in the connect request frame.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task representing the acknowledge operation.</returns>
        Task Acknowledge(
            bool useReceipt = false,
            string transactionId = null,
            IEnumerable<KeyValuePair<string, string>> extraHeaders = null,
            CancellationToken? cancellationToken = null);

        /// <summary>
        /// Acknowledge negative this message.
        /// </summary>
        /// <param name="useReceipt">Indicates whether to require the service to confirm receipt of the message.</param>
        /// <param name="transactionId">The id of the transaction this acknowledge negative makes part of.</param>
        /// <param name="extraHeaders">Non-standard headers to include in the connect request frame.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task representing the acknowledge negative operation.</returns>
        Task AcknowledgeNegative(
            bool useReceipt = false,
            string transactionId = null,
            IEnumerable<KeyValuePair<string, string>> extraHeaders = null,
            CancellationToken? cancellationToken = null);
    }
}