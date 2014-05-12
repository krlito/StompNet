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

namespace StompNet.Models
{
    /// <summary>
    /// Commands as defined by STOMP protocol specification.
    /// </summary>
    public static class StompCommands
    {
        #region Client Commands

        public const string Abort = "ABORT";
        public const string Ack = "ACK";
        public const string Begin = "BEGIN";
        public const string Commit = "COMMIT";
        public const string Connect = "CONNECT";
        public const string Disconnect = "DISCONNECT";
        public const string Nack = "NACK";
        public const string Send = "SEND";
        public const string Stomp = "STOMP";
        public const string Subscribe = "SUBSCRIBE";
        public const string Unsubscribe = "UNSUBSCRIBE";

        private static readonly ISet<string> _clientCommandSet = new HashSet<string> { Send, Subscribe, Unsubscribe, Begin, Commit, Abort, Ack, Nack, Disconnect, Connect, Stomp };

        public static bool IsClientCommand(string command)
        {
            return _clientCommandSet.Contains(command);
        }

        #endregion

        #region Server Commands

        public const string Connected = "CONNECTED";
        public const string Error = "ERROR";
        public const string Message = "MESSAGE";
        public const string Receipt = "RECEIPT";
        public const string Heartbeat = "HEARTBEAT"; // THIS IS AN 'ARTIFICIAL' COMMAND

        private static readonly ISet<string> _serverCommandSet = new HashSet<string> { Connected, Message, Receipt, Error };

        public static bool IsServerCommand(string command)
        {
            return _serverCommandSet.Contains(command);
        }

        #endregion
    }
}
