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
using System.Threading;
using System.Threading.Tasks;
using StompNet.Helpers;
using StompNet.Models.Frames;

namespace StompNet.IO
{
    /// <summary>
    /// Stomp Client.
    /// 
    /// ATTENTION: This is a disposable class.
    /// </summary>
    public class StompClient : IStompClient
    {
        private const string _receiptPrefix = "rcpt-";
        private const string _subscriptionPrefix = "sub-";
        private const string _transactionPrefix = "trx-";

        private readonly bool _cascadeDispose;
        private readonly IStompFrameReader _reader;
        private readonly IStompFrameWriter _writer;

        private readonly IStompFrameObservable _frameObservable;
        private readonly IStompFrameWriter _frameWriter;
        private readonly StompHeartbeatManager _heartbeatManager;
        private readonly ISequenceNumberGenerator _receiptNumberGenerator;
        private readonly ISequenceNumberGenerator _subscriptionNumberGenerator;
        private readonly ISequenceNumberGenerator _transactionNumberGenerator;

        public bool IsStarted 
        { 
            get { return _frameObservable.IsStarted; }
        }

        public string ProtocolVersion
        {
            get { return _frameWriter.ProtocolVersion; }
        }
        
        internal StompClient(
            IStompFrameReader reader, 
            IStompFrameWriter writer,
            TimeSpan? retryInterval = null,
            bool cascadeDispose = false,
            bool useRandomNumberGenerator = false)
        {
            if(reader.ProtocolVersion != writer.ProtocolVersion)
                throw new ArgumentException("Reader and writer MUST use the same protocol version.");

            _cascadeDispose = cascadeDispose;
            _reader = reader;
            _writer = writer;

            _frameObservable = new StompFrameObservable(reader);
            _frameWriter = new StompFrameWriterWithConfirmation(writer, _frameObservable, retryInterval);
            _heartbeatManager = new StompHeartbeatManager(this,  _frameObservable);
            
            if (!useRandomNumberGenerator)
            {
                _receiptNumberGenerator = new SequenceNumberGenerator();
                _subscriptionNumberGenerator = new SequenceNumberGenerator();
                _transactionNumberGenerator = new SequenceNumberGenerator();
            }
            else
            {
                _receiptNumberGenerator = new RandomSequenceNumberGenerator();
                _subscriptionNumberGenerator = _receiptNumberGenerator;
                _transactionNumberGenerator = _receiptNumberGenerator;
            }
        }

        public string GetNextReceiptId()
        {
            return _receiptPrefix + _receiptNumberGenerator.Next();
        }

        public string GetNextSubscriptionId()
        {
            return _subscriptionPrefix + _subscriptionNumberGenerator.Next();
        }

        public string GetNextTransactionId()
        {
            return _transactionPrefix + _transactionNumberGenerator.Next();
        }

        public Task WriteAsync(Frame frame, CancellationToken cancellationToken)
        {
            return _frameWriter.WriteAsync(frame, cancellationToken);
        }

        public void Start(CancellationToken cancellationToken)
        {
            _frameObservable.Start(cancellationToken);
        }

        public IDisposable Subscribe(IObserver<Frame> observer)
        {
            return _frameObservable.Subscribe(observer);
        }

        #region IDisposable Implementation

        private bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if(_disposed) return;

            _frameWriter.Dispose();
            _heartbeatManager.Dispose();
            if(_cascadeDispose)
            {
                _reader.Dispose();
                _writer.Dispose();
            }

            _disposed = true;
        }

        ~StompClient()
        {
            Dispose(false);
        }

        #endregion
    }
}
