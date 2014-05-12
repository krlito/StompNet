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
using System.Threading;
using System.Threading.Tasks;

namespace StompNet
{
    /// <summary>
    /// Stomp connection interface.
    /// </summary>
    public interface IStompConnection : IStompConnectionBase
    {
        /// <summary>
        /// Begin a transaction.
        /// </summary>
        /// <param name="useReceipt">Indicates whether to require the service to confirm receipt of the message.</param>
        /// <param name="extraHeaders">Non-standard headers to include in the connect request frame.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task representing the commit operation. The result of this task is a transaction instance.</returns>
        Task<IStompTransaction> BeginTransactionAsync(
            bool useReceipt = false,
            IEnumerable<KeyValuePair<string, string>> extraHeaders = null,
            CancellationToken? cancellationToken = null);

        /// <summary>
        /// Disconnect from STOMP service.
        /// </summary>
        /// <param name="useReceipt">Indicates whether to require the service to confirm receipt of the message.</param>
        /// <param name="extraHeaders">Non-standard headers to include in the connect request frame.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task representing the disconnect operation.</returns>
        Task DisconnectAsync(
            bool useReceipt = false,
            IEnumerable<KeyValuePair<string, string>> extraHeaders = null,
            CancellationToken? cancellationToken = null);
    }
}