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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using StompNet.Helpers;
using StompNet.IO;
using StompNet.Models;

namespace StompNet
{
    /// <summary>
    /// This is an implementation of the basic common communication commands 
    /// that are used by IStompConnection and IStompTransaction. Basic DRY!
    /// </summary>
    internal class StompConnectionBase
    {
        protected readonly string TransactionId;
        protected readonly StompClient Client;
        
        public StompConnectionBase(StompClient client, string transactionId = null)
        {
            Client = client;
            TransactionId = transactionId;
        }

        public Task SendAsync(
            string destination,
            byte[] content = null,
            string contentType = MediaTypeNames.ApplicationOctet,
            bool useReceipt = false,
            IEnumerable<KeyValuePair<string, string>> extraHeaders = null,
            CancellationToken? cancellationToken = null)
        {
            return Client.WriteSendAsync(
                destination,
                content,
                contentType,
                useReceipt ? Client.GetNextReceiptId() : null,
                TransactionId,
                extraHeaders,
                cancellationToken);
        }

        public Task SendAsync(
            string destination,
            string content,
            Encoding encoding,
            bool useReceipt = false,
            IEnumerable<KeyValuePair<string, string>> extraHeaders = null,
            CancellationToken? cancellationToken = null)
        {
            return Client.WriteSendAsync(
                destination,
                content,
                encoding,
                useReceipt ? Client.GetNextReceiptId() : null,
                TransactionId,
                extraHeaders,
                cancellationToken);
        }

        public Task SendAsync(
            string destination,
            string content,
            bool useReceipt = false,
            IEnumerable<KeyValuePair<string, string>> extraHeaders = null,
            CancellationToken? cancellationToken = null)
        {
            return Client.WriteSendAsync(
                destination,
                content,
                useReceipt ? Client.GetNextReceiptId() : null,
                TransactionId,
                extraHeaders,
                cancellationToken);
        }

        public async Task<IAsyncDisposable> SubscribeAsync(
            IObserver<IStompMessage> observer,
            string destination,
            string ack = null,
            bool useReceipt = true,
            IEnumerable<KeyValuePair<string, string>> extraHeaders = null,
            CancellationToken? cancellationToken = null)
        {
            string subscriptionId = Client.GetNextSubscriptionId();
            IDisposable disposableSubscription = 
                new StompSubscription(
                    Client, 
                    observer, 
                    subscriptionId,
                    ack == StompAckValues.AckClientIndividualValue || ack == StompAckValues.AckClientValue,
                    TransactionId);

            try
            {
                await Client.WriteSubscribeAsync(
                    destination,
                    subscriptionId,
                    useReceipt ? Client.GetNextReceiptId() : null,
                    ack,
                    extraHeaders,
                    cancellationToken);
            }
            catch (Exception)
            {
                disposableSubscription.Dispose();
                throw;
            }

            return new AsyncActionDisposable(
                async (cancelToken) =>
                    {
                        await Client.WriteUnsubscribeAsync(subscriptionId, useReceipt ? Client.GetNextReceiptId() : null, null, cancelToken);
                        disposableSubscription.Dispose();
                    });
        }
    }
}
