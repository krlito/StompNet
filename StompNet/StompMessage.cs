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
using StompNet.IO;
using StompNet.Models.Frames;

namespace StompNet
{
    /// <summary>
    /// Stomp Message.
    /// </summary>
    internal class StompMessage : IStompMessage
    {
        private readonly IStompClient _client;
        private readonly string _transactionId;

        public MessageFrame MessageFrame { get; private set; }
        
        public string ContentType
        {
            get { return MessageFrame.ContentType; }
        }

        public IEnumerable<byte> Content
        {
            get { return MessageFrame.Body; }
        }

        public bool IsAcknowledgeable { get; private set; }

        public string GetContentAsString(Encoding encoding)
        {
            return MessageFrame.GetBodyAsString(encoding);
        }

        public string GetContentAsString()
        {
            return MessageFrame.GetBodyAsString();
        }


        public StompMessage(IStompClient client, MessageFrame messageFrame, bool acknowledgeable, string transactionId = null)
        {
            _client = client;
            MessageFrame = messageFrame;
            IsAcknowledgeable = acknowledgeable;
            _transactionId = transactionId;
        }

        public Task Acknowledge(
            bool useReceipt = false,
            string transactionId = null,
            IEnumerable<KeyValuePair<string, string>> extraHeaders = null,
            CancellationToken? cancellationToken = null)
        {
            if (MessageFrame.Ack == null)
                throw new InvalidOperationException("This message is not acknowledgeable");

            return _client.WriteAckAsync(
                MessageFrame.Ack,
                useReceipt ? _client.GetNextReceiptId() : null,
                transactionId ?? _transactionId,
                extraHeaders,
                cancellationToken);
        }

        public Task AcknowledgeNegative(
            bool useReceipt = false,
            string transactionId = null,
            IEnumerable<KeyValuePair<string, string>> extraHeaders = null,
            CancellationToken? cancellationToken = null)
        {
            if (MessageFrame.Ack == null)
                throw new InvalidOperationException("This message is not acknowledgeable");
            
            return _client.WriteNackAsync(
                MessageFrame.Ack,
                useReceipt ? _client.GetNextReceiptId() : null,
                transactionId ?? _transactionId,
                extraHeaders,
                cancellationToken);
        }

    }
}
