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
using System.Threading.Tasks;
using StompNet.Helpers;
using StompNet.Models;
using StompNet.Models.Frames;

namespace StompNet.IO
{
    /// <summary>
    /// A wrapper of an IStompFrameWriter (plus an IStompFrameObservable) that waits for the receipt 
    /// confirmation frame before returning. If a receipt message is not received, the message is
    /// re-send in intervals of 30 seconds. The default retry interval can be changed at the constructor.
    /// 
    /// This wrapper also handles the case for connect and connected frames. As the second one is the
    /// confirmation of the first one. Receipt header "~connect" is reserved. (quotation marks for clarification).
    /// 
    /// Receipt headers should not be reused. It may produce unexpected results.
    /// 
    /// The receipt header is assumed to already come with the frame to be written on the stream.
    /// </summary>
    public class StompFrameWriterWithConfirmation : IStompFrameWriter
    {
        public static readonly TimeSpan DefRetryInterval = TimeSpan.FromSeconds(30);

        private const string _connectReceiptHeader = "~connect";
        
        private readonly IStompFrameWriter _writer;
        private readonly ConcurrentDictionary<string, TaskCompletionSource<object>> _receiptWaiters;
        private readonly TimeSpan _retryInterval;

        public string ProtocolVersion
        {
            get { return _writer.ProtocolVersion; }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="writer">Frame writer.</param>
        /// <param name="frameObservable">Frame observable to be used to receive the confirmations.</param>
        /// <param name="retryInterval">When sending messages that requires receipt confirmation,
        /// this interval specifies how much time to wait before sending the frame again if 
        /// no receipt is received.</param>
        public StompFrameWriterWithConfirmation(IStompFrameWriter writer, IStompFrameObservable frameObservable, TimeSpan? retryInterval = null)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");

            if (frameObservable == null)
                throw new ArgumentNullException("frameObservable");

            if (frameObservable.ProtocolVersion != writer.ProtocolVersion)
                throw new ArgumentException("Reader and writer MUST use the same protocol version.");

            _writer = _writer as StompSerialFrameWriter ?? new StompSerialFrameWriter(writer);
            frameObservable.SubscribeEx(OnNext, OnError, OnCompleted);

            _receiptWaiters = new ConcurrentDictionary<string, TaskCompletionSource<object>>();
            _retryInterval = retryInterval ?? DefRetryInterval;
        }

        /// <summary>
        /// Write a frame and wait for its receipt frame before returning.
        /// 
        /// This method does not add a receipt header if it is not included already.
        /// If the receipt header is not in the frame headers then it returns after sending the message.
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task WriteAsync(Frame frame, CancellationToken cancellationToken)
        {
            string receipt = frame.Command == StompCommands.Connect ? _connectReceiptHeader : frame.GetHeader(StompHeaders.Receipt);

            if (receipt == null)
            {
                await _writer.WriteAsync(frame, cancellationToken);
            }
            else
            {
                TaskCompletionSource<object> receiptWaiter = new TaskCompletionSource<object>();
                Task task = receiptWaiter.Task;
                _receiptWaiters[receipt] = receiptWaiter;

                do
                {
                    await _writer.WriteAsync(frame, cancellationToken);
                } while (await Task.WhenAny(task, Task.Delay(_retryInterval, cancellationToken)) != task 
                         && !task.IsFaulted && !task.IsCanceled);

                if (task.IsFaulted) throw task.Exception;
                if (task.IsCanceled) throw new OperationCanceledException(cancellationToken);
            }
        }

        private void OnNext(Frame frame)
        {
            string receiptId = 
                  frame.Command == StompCommands.Receipt ? frame.GetHeader(StompHeaders.ReceiptId)
                : frame.Command == StompCommands.Connected ? _connectReceiptHeader
                : null;

            if(receiptId != null)
            {
                TaskCompletionSource<object> tcs;
                if (_receiptWaiters.TryGetValue(receiptId, out tcs))
                {
                    tcs.TrySetResult(null);
                    _receiptWaiters.TryRemove(receiptId, out tcs);
                }
            }
        }

        private void OnError(Exception error)
        {
            _receiptWaiters.Values.Do(tcs => tcs.TrySetException(error));
        }

        private void OnCompleted()
        {
            _receiptWaiters.Values.Do(tcs => tcs.TrySetCanceled());
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            
        }
    }
}
