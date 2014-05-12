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
using System.IO;

namespace StompNet.IO
{
    /// <summary>
    /// STOMP 1.2 Client.
    /// 
    /// ATTENTION: This is a disposable class.
    /// </summary>
    public class Stomp12Client : StompClient
    {

        /// <summary>
        /// Constructor of a client for STOMP 1.2.
        /// </summary>
        /// <param name="stream">Stream for incoming/outgoing data from/to STOMP service.</param>
        /// <param name="retryInterval">When sending messages that require receipt confirmation,
        /// this interval specifies how much time to wait before sending the frame again if 
        /// no receipt is received.</param>
        /// <param name="useRandomNumberGenerator">Flag to indicate random numbers must 
        /// be used when creating sequence numbers for receipts, subscriptions and transactions</param>
        public Stomp12Client(Stream stream, TimeSpan? retryInterval = null, bool useRandomNumberGenerator = false)
            : this (stream, stream, retryInterval, useRandomNumberGenerator)
        {
            
        }

        /// <summary>
        /// Constructor of a client for STOMP 1.2.
        /// </summary>
        /// <param name="inStream">Stream for incoming data from STOMP service.</param>
        /// <param name="outStream">Stream for outgoing data to STOMP service.</param>
        /// <param name="retryInterval">When sending messages that requires receipt confirmation,
        /// this interval specifies how much time to wait before sending the frame again if 
        /// no receipt is received.</param>
        /// <param name="useRandomNumberGenerator">Flag to indicate random numbers must 
        /// be used when creating sequence numbers for receipts, subscriptions and transactions</param>
        public Stomp12Client(Stream inStream, Stream outStream, TimeSpan? retryInterval = null, bool useRandomNumberGenerator = false)
            : base (new Stomp12FrameReader(inStream), new Stomp12FrameWriter(outStream), retryInterval, true, useRandomNumberGenerator)
        {
            
        }
    }
}
