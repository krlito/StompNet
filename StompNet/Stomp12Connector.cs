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
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using StompNet.IO;
using StompNet.Models.Frames;

namespace StompNet
{
    /// <summary>
    /// Connector for Stomp 1.2 protocol.
    /// 
    /// ATTENTION: This is a disposable class.
    /// </summary>
    public sealed class Stomp12Connector : IStompConnector
    {
        private readonly string _host;
        private readonly string _user;
        private readonly string _password;

        private readonly CancellationTokenSource _cts;
        private readonly StompClient _client;
        
        /// <summary>
        /// Constructor of a connector for STOMP 1.2.
        /// </summary>
        /// <param name="inStream">Stream for incoming data from STOMP service.</param>
        /// <param name="outStream">Stream for outgoing data to STOMP service.</param>
        /// <param name="host">Virtual host to connect to STOMP service.</param>
        /// <param name="user">Username to authenticate to STOMP service.</param>
        /// <param name="password">Password to authenticate to STOMP service.</param>
        /// <param name="retryInterval">When sending messages that requires receipt confirmation,
        /// this interval specifies how much time to wait before sending the frame again if 
        /// no receipt is received.</param>
        /// <param name="useRandomNumberGenerator">Flag to indicate random numbers must 
        /// be used when creating sequence numbers for receipts, subscriptions and transactions</param>
        public Stomp12Connector(
            Stream inStream, 
            Stream outStream, 
            string host, 
            string user = null, 
            string password = null,
            TimeSpan? retryInterval = null,
            bool useRandomNumberGenerator = false)
        {
            if(host == null)
                throw new ArgumentNullException("host");

            _cts = new CancellationTokenSource();
            _client = new Stomp12Client(inStream, outStream, retryInterval, useRandomNumberGenerator);

            _host = host;
            _user = user;
            _password = password;
        }

        /// <summary>
        /// Constructor of a connector for STOMP 1.2.
        /// </summary>
        /// <param name="stream">Stream for incoming/outgoing data from/to STOMP service.</param>
        /// <param name="host">Virtual host to connect to STOMP service.</param>
        /// <param name="user">Username to authenticate to STOMP service.</param>
        /// <param name="password">Password to authenticate to STOMP service.</param>
        /// <param name="retryInterval">When sending messages that require receipt confirmation,
        /// this interval specifies how much time to wait before sending the frame again if 
        /// no receipt is received.</param>
        /// <param name="useRandomNumberGenerator">Flag to indicate random numbers must 
        /// be used when creating sequence numbers for receipts, subscriptions and transactions</param>
        public Stomp12Connector(
            Stream stream, 
            string host, 
            string user = null, 
            string password = null,
            TimeSpan? retryInterval = null,
            bool useRandomNumberGenerator = false)
            : this(stream, stream, host, user, password, retryInterval, useRandomNumberGenerator)
        { }


        /// <summary>
        /// Connect to the STOMP 1.2 service.
        /// </summary>
        /// <param name="extraHeaders">Non-standard headers to include in the connect request frame.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task representing the connect operation.</returns>
        public async Task<IStompConnection> ConnectAsync(
            IEnumerable<KeyValuePair<string, string>> extraHeaders = null,
            Heartbeat heartbeat = null,
            CancellationToken? cancellationToken = null)
        {
            _client.Start(_cts.Token);

            await _client.WriteConnectAsync(_host, _user, _password, heartbeat ?? Heartbeat.NoHeartbeat, extraHeaders, cancellationToken);

            return new StompConnection(_client);
        }

        public void Dispose()
        {
            _cts.Cancel();
            _cts.Dispose();
            _client.Dispose();
        }
    }
}
