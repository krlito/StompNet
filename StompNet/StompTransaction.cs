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
    /// Stomp Transaction.
    /// </summary>
    internal class StompTransaction : StompConnectionBase, IStompTransaction
    {
        public string Id
        {
            get { return TransactionId; }
        }
        
        public StompTransaction(StompClient client, string id)
            : base(client, id)
        {
        }

        public Task CommitAsync(
            bool useReceipt = false,
            IEnumerable<KeyValuePair<string, string>> extraHeaders = null,
            CancellationToken? cancellationToken = null)
        {
            return Client.WriteCommitAsync(
                Id,
                useReceipt ? Client.GetNextReceiptId() : null,
                extraHeaders,
                cancellationToken);
        }

        public Task AbortAsync(
            bool useReceipt = false,
            IEnumerable<KeyValuePair<string, string>> extraHeaders = null,
            CancellationToken? cancellationToken = null)
        {
            return Client.WriteAbortAsync(
                Id,
                useReceipt ? Client.GetNextReceiptId() : null,
                extraHeaders,
                cancellationToken);
        }
    }
}
