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
using StompNet.Models.Frames;

namespace StompNet.IO
{
    /// <summary>
    /// Interface for a frame reader in an 'observer pattern' fashion.
    /// </summary>
    public interface IStompFrameObservable : IObservable<Frame>
    {
        bool IsStarted { get; }
        string ProtocolVersion { get; }

        /// <summary>
        /// Start processing incoming frames.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token that may be used to stop the processing of incoming frames.</param>
        void Start(CancellationToken cancellationToken);
    }

    /// <summary>
    /// IStompFrameObservable extensions.
    /// </summary>
    public static class StompFrameObservableExtensions
    {
        /// <summary>
        /// Start processing incoming frames.
        /// </summary>
        public static void Start(this IStompFrameObservable stompFrameObservable)
        {
            stompFrameObservable.Start(CancellationToken.None);
        }
    }
}