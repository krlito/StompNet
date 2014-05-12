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
using StompNet.IO;

namespace StompNet
{
    /// <summary>
    /// Stomp Connection.
    /// </summary>
    internal class StompConnection : StompConnectionBase, IStompConnection
    {
        public StompConnection(StompClient client)
            : base(client)
        {

        }

        public async Task<IStompTransaction> BeginTransactionAsync(
            bool useReceipt = false,
            IEnumerable<KeyValuePair<string, string>> extraHeaders = null,
            CancellationToken? cancellationToken = null)
        {
            string transactionId = Client.GetNextTransactionId();
            await Client.WriteBeginAsync(
                transactionId,
                useReceipt ? Client.GetNextReceiptId() : null,
                extraHeaders,
                cancellationToken);

            return new StompTransaction(Client, transactionId);
        }

        public Task DisconnectAsync(
            bool useReceipt = false,
            IEnumerable<KeyValuePair<string, string>> extraHeaders = null,
            CancellationToken? cancellationToken = null)
        {
            return Client.WriteDisconnectAsync(
                useReceipt ? Client.GetNextReceiptId() : null,
                extraHeaders,
                cancellationToken);
        }
    }
}
