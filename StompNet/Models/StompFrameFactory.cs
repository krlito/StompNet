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
using System.Linq;
using System.Text;
using StompNet.Helpers;
using StompNet.Models.Frames;

namespace StompNet.Models
{
    /// <summary>
    /// This is a class to ease the making of any client frame.
    /// </summary>
    public static class StompFrameFactory
    {
        public static Frame CreateConnect(
            string acceptVersion,
            string host = null,
            string login = null,
            string passcode = null,
            Heartbeat heartbeat = null,
            IEnumerable<KeyValuePair<string, string>> extraHeaders = null)
        {
            // This should be commented when multiversion is implemented.
            if (host == null)
                throw new ArgumentNullException("host");

            var headers = new List<KeyValuePair<string, string>>(5);
            headers.Add(new KeyValuePair<string, string>(StompHeaders.AcceptVersion, acceptVersion));
            /*if (host != null)*/ headers.Add(new KeyValuePair<string, string>(StompHeaders.Host, host));
            if (login != null) headers.Add(new KeyValuePair<string, string>(StompHeaders.Login, login));
            if (passcode != null) headers.Add(new KeyValuePair<string, string>(StompHeaders.Passcode, passcode));
            if (heartbeat != null) headers.Add(new KeyValuePair<string, string>(StompHeaders.Heartbeat, heartbeat.RawHeartbeat));

            return new Frame(StompCommands.Connect, extraHeaders == null ? headers : headers.Concat(extraHeaders));
        }

        public static Frame CreateSend(
            string destination,
            byte[] body = null,
            string contentType = MediaTypeNames.ApplicationOctet,
            string receipt = null,
            string transaction = null,
            IEnumerable<KeyValuePair<string, string>> extraHeaders = null)
        {
            if (destination == null)
                throw new ArgumentNullException("destination");

            var headers = new List<KeyValuePair<string, string>>(5);
            headers.Add(new KeyValuePair<string, string>(StompHeaders.Destination, destination));
            if (receipt != null) headers.Add(new KeyValuePair<string, string>(StompHeaders.Receipt, receipt));
            if (contentType != null) headers.Add(new KeyValuePair<string, string>(StompHeaders.ContentType, contentType));
            if (transaction != null) headers.Add(new KeyValuePair<string, string>(StompHeaders.Transaction, transaction));
            headers.Add(new KeyValuePair<string, string>(StompHeaders.ContentLength, body != null ? body.Length.ToString() : "0"));

            return new Frame(StompCommands.Send, extraHeaders == null ? headers : headers.Concat(extraHeaders), body);
        }

        public static Frame CreateSend(
            string destination,
            string body,
            Encoding encoding,
            string receipt = null,
            string transaction = null,
            IEnumerable<KeyValuePair<string, string>> extraHeaders = null)
        {
            return CreateSend(destination, 
                encoding.GetBytes(body),
                MediaTypeNames.TextPlain + ";charset=" + encoding.WebName, 
                receipt, 
                transaction,
                extraHeaders);
        }

        public static Frame CreateSend(
            string destination,
            string body,
            string receipt = null,
            string transaction = null,
            IEnumerable<KeyValuePair<string, string>> extraHeaders = null)
        {
            return CreateSend(destination, 
                body, 
                Encoding.UTF8, 
                receipt, 
                transaction, 
                extraHeaders);
        }

        public static Frame CreateSubscribe(
            string destination,
            string id,
            string receipt = null,
            string ack = null,
            IEnumerable<KeyValuePair<string, string>> extraHeaders = null)
        {
            if (destination == null)
                throw new ArgumentNullException("destination");

            if (id == null)
                throw new ArgumentNullException("id");

            if (ack != null)
                StompAckValues.ThrowIfInvalidAckValue(ack);

            var headers = new List<KeyValuePair<string, string>>(4);
            headers.Add(new KeyValuePair<string, string>(StompHeaders.Destination, destination));
            headers.Add(new KeyValuePair<string, string>(StompHeaders.Id, id));
            if (receipt != null) headers.Add(new KeyValuePair<string, string>(StompHeaders.Receipt, receipt));
            if (ack != null) headers.Add(new KeyValuePair<string, string>(StompHeaders.Ack, ack));

            return new Frame(StompCommands.Subscribe, extraHeaders == null ? headers : headers.Concat(extraHeaders));
        }

        public static Frame CreateUnsubscribe(
            string id,
            string receipt = null,
            IEnumerable<KeyValuePair<string, string>> extraHeaders = null)
        {

            if (id == null)
                throw new ArgumentNullException("id");

            var headers = new List<KeyValuePair<string, string>>(2);
            headers.Add(new KeyValuePair<string, string>(StompHeaders.Id, id));
            if (receipt != null) headers.Add(new KeyValuePair<string, string>(StompHeaders.Receipt, receipt));

            return new Frame(StompCommands.Unsubscribe, extraHeaders == null ? headers : headers.Concat(extraHeaders));
        }

        public static Frame CreateAck(
            string id,
            string receipt = null,
            string transaction = null,
            IEnumerable<KeyValuePair<string, string>> extraHeaders = null)
        {
            if (id == null)
                throw new ArgumentNullException("id");

            var headers = new List<KeyValuePair<string, string>>(3);
            headers.Add(new KeyValuePair<string, string>(StompHeaders.Id, id));
            if (receipt != null) headers.Add(new KeyValuePair<string, string>(StompHeaders.Receipt, receipt));
            if (transaction != null) headers.Add(new KeyValuePair<string, string>(StompHeaders.Transaction, transaction));

            return new Frame(StompCommands.Ack, extraHeaders == null ? headers : headers.Concat(extraHeaders));
        }

        public static Frame CreateNack(
            string id,
            string receipt = null,
            string transaction = null,
            IEnumerable<KeyValuePair<string, string>> extraHeaders = null)
        {
            if (id == null)
                throw new ArgumentNullException("id");

            var headers = new List<KeyValuePair<string, string>>(3);
            headers.Add(new KeyValuePair<string, string>(StompHeaders.Id, id));
            if (receipt != null) headers.Add(new KeyValuePair<string, string>(StompHeaders.Receipt, receipt));
            if (transaction != null) headers.Add(new KeyValuePair<string, string>(StompHeaders.Transaction, transaction));

            return new Frame(StompCommands.Nack, extraHeaders == null ? headers : headers.Concat(extraHeaders));
        }

        public static Frame CreateBegin(
            string transaction,
            string receipt = null,
            IEnumerable<KeyValuePair<string, string>> extraHeaders = null)
        {
            if (transaction == null)
                throw new ArgumentNullException("transaction");

            var headers = new List<KeyValuePair<string, string>>(2);
            headers.Add(new KeyValuePair<string, string>(StompHeaders.Transaction, transaction));
            if (receipt != null) headers.Add(new KeyValuePair<string, string>(StompHeaders.Receipt, receipt));
            
            return new Frame(StompCommands.Begin, extraHeaders == null ? headers : headers.Concat(extraHeaders));
        }

        public static Frame CreateCommit(
            string transaction,
            string receipt = null,
            IEnumerable<KeyValuePair<string, string>> extraHeaders = null)
        {
            if (transaction == null)
                throw new ArgumentNullException("transaction");

            var headers = new List<KeyValuePair<string, string>>(2);
            headers.Add(new KeyValuePair<string, string>(StompHeaders.Transaction, transaction));
            if (receipt != null) headers.Add(new KeyValuePair<string, string>(StompHeaders.Receipt, receipt));

            return new Frame(StompCommands.Commit, extraHeaders == null ? headers : headers.Concat(extraHeaders));
        }

        public static Frame CreateAbort(
            string transaction,
            string receipt = null,
            IEnumerable<KeyValuePair<string, string>> extraHeaders = null)
        {
            if (transaction == null)
                throw new ArgumentNullException("transaction");

            var headers = new List<KeyValuePair<string, string>>(2);
            headers.Add(new KeyValuePair<string, string>(StompHeaders.Transaction, transaction));
            if (receipt != null) headers.Add(new KeyValuePair<string, string>(StompHeaders.Receipt, receipt));

            return new Frame(StompCommands.Abort, extraHeaders == null ? headers : headers.Concat(extraHeaders));
        }

        public static Frame CreateDisconnect(
            string receipt = null,
            IEnumerable<KeyValuePair<string, string>> extraHeaders = null)
        {

            var headers = new List<KeyValuePair<string, string>>(1);
            if (receipt != null) headers.Add(new KeyValuePair<string, string>(StompHeaders.Receipt, receipt));

            return new Frame(StompCommands.Disconnect, extraHeaders == null ? headers : headers.Concat(extraHeaders));
        }
    }
}
