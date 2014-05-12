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
using System.Collections.Concurrent;
using System.Threading;
using StompNet.IO;
using StompNet.Models;
using StompNet.Models.Frames;

namespace StompNet
{
    /// <summary>
    /// Stomp Subscription.
    /// 
    /// This is a class to filter all elements incoming from the server.
    /// It only allows the observer (received in the constructor) to receive
    /// messages from a given subscription id.
    /// 
    /// This class does NOT do any communication (send/receive). Anyway, it
    /// receives an IStompClient because IStompMessages may need it.
    /// </summary>
    internal sealed class StompSubscription : IObserver<Frame>, IDisposable
    {
        // I think that if I don't root my subscriptions and the user does not keep 
        // a reference then they may be garbage-collected.
        private static readonly ConcurrentDictionary<StompSubscription, StompSubscription> Root = new ConcurrentDictionary<StompSubscription, StompSubscription>();

        private readonly IStompClient _client;
        private readonly IObserver<IStompMessage> _observer;
        private readonly string _id;
        private readonly bool _acknowledgeableMessages;
        private readonly string _transactionId;
        private readonly IDisposable _disposableSubscription;

        public StompSubscription(IStompClient client, IObserver<IStompMessage> observer, string id, bool acknowledgeableMessages, string transactionId = null)
        {
            _client = client;
            _observer = observer;
            _id = id;
            _transactionId = transactionId;
            _acknowledgeableMessages = acknowledgeableMessages;
            _disposableSubscription = client.Subscribe(this);
            
            Root[this] = this;
        }


        public void OnNext(Frame value)
        {
            if (value.Command == StompCommands.Message)
            {
                MessageFrame messageFrame = value as MessageFrame ?? new MessageFrame(value.Headers, value.BodyArray);

                if (messageFrame.Subscription == _id)
                {
                    _observer.OnNext(new StompMessage(_client, messageFrame, _acknowledgeableMessages, _transactionId));
                }
            }
        }
        
        public void OnError(Exception error)
        {
            _observer.OnError(error);
            Dispose(false);
        }

        public void OnCompleted()
        {
            _observer.OnCompleted();
            Dispose(false);
        }

        #region IDisposable Implementation
        
        private int _disposed = 0;
        
        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) != 0)
                return;

            _disposableSubscription.Dispose();
            StompSubscription _;
            Root.TryRemove(this, out _);

            if(disposing)
            {
                _observer.OnCompleted();
            }
        }

        //This may not be called before disposing because I'm rooting all subscriptions until they are disposed.
        ~StompSubscription()
        {
            Dispose(false);
        }

        #endregion
    }
}
