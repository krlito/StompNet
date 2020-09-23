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

using StompNet.Helpers;
using System.Collections.Generic;

namespace StompNet.Models.Frames
{
    /// <summary>
    /// Class representing a STOMP message frame.
    /// </summary>
    public class MessageFrame : Frame
    {
        public string Destination { get; private set; }

        public string MessageId { get; private set; }

        public string Subscription { get; private set; }

        public string Ack { get; private set; }

        public string ContentType { get; private set; }

        public int ContentLength { get; private set; }

        internal MessageFrame(IEnumerable<KeyValuePair<string, string>> headers, byte[] body = null) 
            : base (StompCommands.Message, headers, body)
        {
            int missingHeaders = 5;

            foreach(var header in Headers)
            {
                if (Destination == null && header.Key == StompHeaders.Destination)
                {
                    Destination = header.Value;
                    missingHeaders--;
                }
                else if (MessageId == null && header.Key == StompHeaders.MessageId)
                {
                    MessageId = header.Value;
                    missingHeaders--;
                }
                else if (Subscription == null && header.Key == StompHeaders.Subscription)
                {
                    Subscription = header.Value;
                    missingHeaders--;
                }
                else if (ContentType == null && header.Key == StompHeaders.ContentType)
                {
                    ContentType = header.Value;
                    missingHeaders--;
                }
                else if (Ack == null && header.Key == StompHeaders.Ack)
                {
                    Ack = header.Value;
                    missingHeaders--;
                }

                if(missingHeaders == 0) break;
            }

            if(missingHeaders != 0)
            {
                if (string.IsNullOrEmpty(Destination))
                    ThrowMandatoryHeaderException(StompHeaders.Destination);
                if (string.IsNullOrEmpty(MessageId))
                    ThrowMandatoryHeaderException(StompHeaders.MessageId);
                if (string.IsNullOrEmpty(Subscription))
                    ThrowMandatoryHeaderException(StompHeaders.Subscription);
                if(string.IsNullOrEmpty(ContentType))
                    ContentType = MediaTypeNames.ApplicationOctet;
            }

            ContentLength = BodyArray.Length;
        }
    }
}
