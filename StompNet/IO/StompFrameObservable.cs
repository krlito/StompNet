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
using StompNet.Exceptions;
using StompNet.Helpers;
using StompNet.Models;
using StompNet.Models.Frames;

namespace StompNet.IO
{
    /// <summary>
    /// Implementation of a wrapper of an IStompFrameReader into an IStompFrameObservable.
    /// </summary>
    public class StompFrameObservable : IStompFrameObservable
    {
        private readonly ConcurrentDictionary<IObserver<Frame>, IObserver<Frame>> _observers;
        private readonly IStompFrameReader _reader;

        /// <summary>
        /// Status of the processing of incoming frames.
        /// 0 => Created, 1 => Started
        /// </summary>
        private int _status;

        public bool IsStarted
        {
            get { return (_status == 1); }
        }

        public string ProtocolVersion
        {
            get { return _reader.ProtocolVersion; }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="reader">Frame reader.</param>
        public StompFrameObservable(IStompFrameReader reader)
        {
            _observers = new ConcurrentDictionary<IObserver<Frame>, IObserver<Frame>>();
            _reader = reader;
            _status = 0;
        }

        public void Start(CancellationToken cancellationToken)
        {
            //Avoid multiple starts.
            if (Interlocked.CompareExchange(ref _status, 1, 0) != 0)
                throw new InvalidOperationException("The observable is already started.");

            Task.Factory.StartNew(
                async () => await ReadFrames(cancellationToken), 
                cancellationToken,
                TaskCreationOptions.None, //TaskCreationOptions.LongRunning, 
                TaskScheduler.Default);
        }

        private async Task ReadFrames(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    Frame value = await _reader.ReadFrameAsync(cancellationToken);

                    if (value.Command == StompCommands.Error)
                        throw new ErrorFrameException(new ErrorFrame(value.Headers, value.BodyArray));

                    _observers.Values.DoIgnoringExceptions(o => o.OnNext(value));
                }
            }
            catch (OperationCanceledException)
            {
                _observers.Values.DoIgnoringExceptions(o => o.OnCompleted());
            }
            catch (Exception e)
            {
                _observers.Values.DoIgnoringExceptions(o => o.OnError(e));
            }
        }

        public IDisposable Subscribe(IObserver<Frame> observer)
        {
            _observers[observer] = observer;
            
            return new ActionDisposable(() => _observers.TryRemove(observer, out observer));
        }
    }
}
