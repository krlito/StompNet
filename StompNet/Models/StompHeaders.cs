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
 
namespace StompNet.Models
{
    /// <summary>
    /// Frame headers as defined by STOMP protocol specification.
    /// </summary>
    public static class StompHeaders
    {
        public const string AcceptVersion = "accept-version";
        public const string Ack = "ack";
        public const string ContentLength = "content-length";
        public const string ContentType = "content-type";
        public const string Destination = "destination";
        public const string Heartbeat = "heart-beat";
        public const string Host = "host";
        public const string Id = "id";
        public const string Login = "login";
        public const string Message = "message";
        public const string MessageId = "message-id";
        public const string Passcode = "passcode";
        public const string Receipt = "receipt";
        public const string ReceiptId = "receipt-id";
        public const string Server = "server";
        public const string Session = "session";
        public const string Subscription = "subscription";
        public const string Transaction = "transaction";
        public const string Version = "version";
    }
}