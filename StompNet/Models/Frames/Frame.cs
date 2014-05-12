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

namespace StompNet.Models.Frames
{
    /// <summary>
    /// Class representing a STOMP FRAME.
    /// 
    /// No validation is done here except a command string is required.
    /// All validations must be done in inheritors.
    /// 
    /// This object is not immutable, BUT should be used as if it were.
    /// </summary>
    public class Frame
    {
        /// <summary>
        /// Value used when the body is empty.
        /// </summary>
        public static readonly byte[] EmptyBody = new byte[0];

        /// <summary>
        /// Value to be used when a heartbeat is received.
        /// </summary>
        public static readonly Frame HeartbeatFrame = new Frame(StompCommands.Heartbeat, null);

        /// <summary>
        /// Frame command.
        /// </summary>
        public string Command { get; private set; }

        /// <summary>
        /// Frame headers.
        /// </summary>
        public IEnumerable<KeyValuePair<string, string>> Headers { get; private set; }

        /// <summary>
        /// Frame body.
        /// </summary>
        public IEnumerable<byte> Body { get { return BodyArray; } }

        /// <summary>
        /// Frame body as a byte array. To be used internally by the library.
        /// </summary>
        internal byte[] BodyArray { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="command">Frame command.</param>
        /// <param name="headers">Frame headers.</param>
        /// <param name="body">Frame body.</param>
        public Frame(string command, IEnumerable<KeyValuePair<string, string>> headers, byte[] body = null)
        {
            if(command == null)
                throw new ArgumentNullException("command");

            Command = command;
            Headers = headers ?? Enumerable.Empty<KeyValuePair<string, string>>();
            BodyArray = body == null || body.Length == 0 ? EmptyBody : body;
        }

        /// <summary>
        /// Create and throws the exception used when a mandatory header is not included.
        /// </summary>
        /// <param name="headerName">Name of the mandatory header.</param>
        protected static void ThrowMandatoryHeaderException(string headerName)
        {
            throw new ArgumentException("'" + headerName + "' header is mandatory.", headerName);
        }

        /// <summary>
        /// Gets the value of the first ocurrence of the specified header.
        /// </summary>
        /// <param name="name">Name of the header.</param>
        /// <returns>The value of the header or null if it is not found.</returns>
        public string GetHeader(string name)
        {
            return Headers.Where(h => h.Key == name).Select(h => h.Value).FirstOrDefault();
        }

        /// <summary>
        /// Gets all the values of the specified header.
        /// </summary>
        /// <param name="name">Name of the header.</param>
        /// <returns>An enumerable of the values of the header.</returns>
        public IEnumerable<string> GetAllHeaderValues(string name)
        {
            return Headers.Where(h => h.Key == name).Select(h => h.Value);
        }

        /// <summary>
        /// Get body as a string using the specified encoding.
        /// </summary>
        /// <param name="encoding">Encoding to be used to decode the message body.</param>
        /// <returns>A string representing the message body decoded using the specified encoding.</returns>
        public string GetBodyAsString(Encoding encoding)
        {
            if(encoding == null)
                throw new ArgumentNullException("encoding");

            return BodyArray != null ? encoding.GetString(BodyArray) : null;
        }

        /// <summary>
        /// Get body as a string using UTF-8 encoding.
        /// </summary>
        /// <returns>A string representing the message body decoded using UTF-8 encoding.</returns>
        public string GetBodyAsString()
        {
            return GetBodyAsString(Encoding.UTF8);
        }

        /// <summary>
        /// String representation of the Frame.
        /// </summary>
        /// <returns>A string representing the frame.</returns>
        public override string ToString()
        {
            StringBuilder str = new StringBuilder();

            str.AppendLine(Command);
            Headers.Do(kvp => str.AppendLine(kvp.Key + ":" + kvp.Value));
            str.AppendLine();

            string contentType = GetHeader(StompHeaders.ContentType);
            if (contentType != null && contentType.StartsWith("text/") && (contentType.Contains("utf-8") || contentType.Contains("ascii")))
            {
                str.Append(Encoding.UTF8.GetString(BodyArray.Take(60).ToArray()));
                if (BodyArray.Length > 60) str.Append(" ...");
            }
            else
            {
                str.Append(string.Join(" ", BodyArray.Take(20).Select(b => b.ToString("X2"))));
                if (BodyArray.Length > 20) str.Append(" ...");
            }

            str.AppendLine();
            return str.ToString();
        }
    }
}
